/// <reference path="../typings/tsd.d.ts" />
/// <reference path="../typings_custom/virtual-dom.d.ts" />

import createElement = require("virtual-dom/create-element");
import jQuery = require("jquery");

describe("integration test", () => {

    it("returns something", done => {
        jQuery.getJSON("http://localhost:5000/test?template=test/Templates/simple.html")
            .done(result => {
                
                expect(result).not.toBeNull();

                var rootNode = createElement(<VNode>result);
                expect(rootNode).not.toBeNull();

                done();
            });
    });
});