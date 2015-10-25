/// <reference path="../typings/tsd.d.ts" />
/// <reference path="../typings_custom/virtual-dom.d.ts" />

import createElement = require("virtual-dom/create-element");
import diff = require("virtual-dom/diff");
import patch = require("virtual-dom/patch");
import jQuery = require("jquery");

function doRequest(data: any) {
    return jQuery.ajax(
    {
        url: "http://localhost:5000/test?template=test/Templates/simple.html",
        data: JSON.stringify(data),
        contentType: "application/json",
        type: "POST"
    });
}

describe("integration test", () => {

    it("returns something", done => {

        var data = { url: "http://terrific.net", content:"Test" };
        var data2 = { url: "http://terrific.net", content:"Test2" };
        doRequest(data)
            .done(result => {

                expect(result).not.toBeNull();
                var vNode = <VNode>result;
                var rootNode = createElement(vNode.children[0]);
                document.body.appendChild(rootNode);

                doRequest(data2).done(result => {

                    var patches = diff(vNode, (<VNode>result).children[0]);
                    rootNode = patch(rootNode, patches);

                    expect(rootNode).not.toBeNull();

                    var a = <HTMLAnchorElement>jQuery("a", rootNode)[0];
                    expect(a.href).toBe(data2.url);

                    done();
                });
            });
    });
});