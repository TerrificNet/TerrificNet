/// <reference path="../typings/index.d.ts" />

import * as tn from "../index"

import * as vdom from "virtual-dom";
import * as idom from "incremental-dom";

function handleFail(done: () => void): (data: any) => void {
   return data => {
      fail(data);
      done();
   };
}

var root = "http://localhost:5000/";

describe("integration test", () => {

   var url = root + "test?template=test/Templates/simple.html";
   
   it("simple.html", done => {

      var data = { url: "http://terrific.net", content: "Test" };
      var data2 = { url: "http://terrific2.net", content: "Test2" };

      var fail = handleFail(done);
      
      tn.View.createFromVDom(tn.post<vdom.VTree>(url, data)).then(view => {

         var a = view.node.children[0];
         expect(a).not.toBeNull();
         expect(a.getAttribute("href")).toBe(data.url);
         expect(a.firstChild.nodeValue).toBe(data.content);

         view.updateAsync(tn.post<vdom.VTree>(url, data2)).then(() => {

            var a2 = view.node.children[0];
            expect(a2).toBe(a);
            expect(a2.getAttribute("href")).toBe(data2.url);
            expect(a2.firstChild.nodeValue).toBe(data2.content);

            done();
         }, fail);
      }, fail);
   });
});

describe("integration test incremental dom", () => {
   it("simple.html", done => {
      var data = { url: "http://terrific.net", content: "Test" };
      var data2 = { url: "http://terrific2.net", content: "Test2" };

      var scope = {
         click: () => {}
      };

      var rootNode = document.createElement("div");
      var view = new tn.IncrementalView(rootNode);

      var url = root + "test/incremental?template=test/Templates/client_scope.html";

      tn.post<string>(url, data, false).then(template => {
         
         view.executeFuncFromTemplate(template, scope);
         
         var node = (rootNode.children[0]) as HTMLElement;

         var a = node.children[0] as HTMLElement;
         expect(a).not.toBeNull();
         expect(a.getAttribute("href")).toBe(data.url);
         expect(a.firstChild.nodeValue).toBe(data.content);
         expect(a["onclick"]).toBe(scope.click);

         tn.post<string>(url, data2, false).then(template2 => {

            view.executeFuncFromTemplate(String(template2), scope);

            node = (rootNode.children[0]) as HTMLElement;

            var a2 = node.children[0];
            expect(a2).toBe(a);
            expect(a2.getAttribute("href")).toBe(data2.url);
            expect(a2.firstChild.nodeValue).toBe(data2.content);

            done();
         }, fail);
         
      }, fail);

   });

});

describe("incremetal dom applies missing properties", () => {
   it("client_scope.html", done => {
      var data = { url: "http://terrific.net", content: "Test" };

      var scope = {
         click: () => { }
      };

      var rootNode = document.createElement("div");
      var view = new tn.IncrementalView(rootNode);

      var url = root + "test/text?template=test/Templates/client_scope.html";
      var url2 = root + "test/incremental?template=test/Templates/client_scope.html";

      tn.post<string>(url, data, false)
         .then(html => {
            rootNode.innerHTML = html;

            tn.post<string>(url2, data, false)
               .then(template2 => {
                  view.executeFuncFromTemplate(String(template2), scope);
                  var node = (rootNode.children[0]) as HTMLElement;

                  var a2 = node.children[0] as HTMLElement;
                  expect(a2.firstChild).not.toBeNull();
                  expect(a2.firstChild.nodeValue).toBe(data.content);
                  expect(a2.getAttribute("href")).toBe(data.url);
                  expect(a2["onclick"]).toBe(scope.click);

                  done();

                  },
                  fail);
         }, fail);
   });
});