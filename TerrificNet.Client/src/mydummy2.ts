/// <reference path="../typings/jquery/jquery.d.ts" />

//import dummy = require("mydummy");

namespace Gugus {
    export class MyDummy2 {
        doSomething(): string {
            $("#gugus").add($("<h1 />"));

            const s = new MyDummy();
            return s.doSomething();
        }
    }
}