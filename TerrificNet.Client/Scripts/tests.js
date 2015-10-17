/// <reference path="typings/jasmine/jasmine.d.ts" />
/// <reference path="dummy.ts" />
describe("basic test", function () {
    var dummy = UnderTest.Dummy;
    it("dummy do returns hallo", function () {
        var underTest = new dummy();
        var result = underTest.Do();
        expect(result).toBe("hallo");
    });
    it("something", function () {
        var underTest = new dummy();
        var result = underTest.Do();
        expect(result).toBe("hallo");
    });
});
//# sourceMappingURL=tests.js.map