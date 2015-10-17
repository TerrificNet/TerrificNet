var UnderTest;
(function (UnderTest) {
    var Dummy = (function () {
        function Dummy() {
        }
        Dummy.prototype.Do = function () {
            return "hallo";
        };
        return Dummy;
    })();
    UnderTest.Dummy = Dummy;
})(UnderTest || (UnderTest = {}));
//# sourceMappingURL=dummy.js.map