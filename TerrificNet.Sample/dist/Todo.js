"use strict";
var thtml = require("terrific-net");
var TodoComponent = (function () {
    function TodoComponent(node) {
        this.view = new thtml.IncrementalView(document.body);
    }
    TodoComponent.prototype.add = function (description) {
        var _this = this;
        thtml.post('/todo', description, false).then(function (template) {
            _this.view.executeFuncFromTemplate(template);
        });
    };
    return TodoComponent;
}());
exports.TodoComponent = TodoComponent;
//# sourceMappingURL=Todo.js.map