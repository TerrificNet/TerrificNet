/// <reference path="../typings/tsd.d.ts" />
/// <reference path="../typings_custom/virtual-dom.d.ts" />

import thtml = require("../src/view");
import http = require("../src/request");

describe("integration test", () => {

    var url = "http://localhost:5000/test?template=test/Templates/simple.html";

    it("returns something", done => {

        var data = { url: "http://terrific.net", content:"Test" };
        var data2 = { url: "http://terrific.net", content: "Test2" };

        thtml.View.createFromVDom(http.post<VTree>(url, data)).then(view => {

            var a = view.node.children[0];
            expect(a).not.toBeNull();
            expect(a.getAttribute("href")).toBe(data.url);

            view.updateAsync(http.post<VTree>(url, data2)).then(() => {

                var a2 = view.node.children[0];
                expect(a2).toBe(a);
                expect(a2.getAttribute("href")).toBe(data2.url);

                done();
            });
        });
    });
});