/// <reference path="../typings/jasmine/jasmine.d.ts" />
/// <reference path="../src/mydummy.ts" />

describe('mydummytest', () => {
    it("returns something", () => {
        var s = new Gugus.MyDummy();
        expect(s.doSomething()).toBe("hallo");
    });
});