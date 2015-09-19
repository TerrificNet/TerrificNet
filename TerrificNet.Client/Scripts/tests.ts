/// <reference path="typings/jasmine/jasmine.d.ts" />
/// <reference path="dummy.ts" />

describe("basic test", () => {
    let dummy = UnderTest.Dummy;

    it("dummy do returns hallo", () => {

        var underTest = new dummy();
        var result = underTest.Do();

        expect(result).toBe("hallo");

    });

    it("something", () => {

        var underTest = new dummy();
        var result = underTest.Do();

        expect(result).toBe("hallo");
    });

});