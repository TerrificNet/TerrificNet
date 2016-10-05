/// <reference path="../typings/tsd.d.ts" />
/// <reference path="../typings_custom/virtual-dom.d.ts" />

import thtml = require("../src/view");
import ihtml = require("../src/incrementalView");
import http = require("../src/http");

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

      thtml.View.createFromVDom(http.post<VTree>(url, data)).then(view => {

         var a = view.node.children[0];
         expect(a).not.toBeNull();
         expect(a.getAttribute("href")).toBe(data.url);
         expect(a.firstChild.nodeValue).toBe(data.content);

         view.updateAsync(http.post<VTree>(url, data2)).then(() => {

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

      var rootNode = document.createElement("div");
      var view = new ihtml.IncrementalView(rootNode);

      var url = root + "test/incremental?template=test/Templates/simple.html";

      http.post(url, data, false).then(template => {
         console.log(template);

         view.executeFuncFromTemplate(String(template));

         var node = (rootNode.children[0]) as HTMLElement;

         var a = node.children[0];
         expect(a).not.toBeNull();
         expect(a.getAttribute("href")).toBe(data.url);
         expect(a.firstChild.nodeValue).toBe(data.content);

         http.post(url, data2, false).then(template2 => {
            view.executeFuncFromTemplate(String(template2));

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