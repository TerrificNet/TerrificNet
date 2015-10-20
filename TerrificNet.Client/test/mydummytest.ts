/// <reference path="../typings/jasmine/jasmine.d.ts" />
/// <reference path="../src/mydummy.ts" />
//import Mydummy = require("../src/mydummy");

describe('mydummytest', () => {
    it("returns something", () => {
        var s = new Gugus.MyDummy();
        expect(s.doSomething()).toBe("hallo");
    });
});