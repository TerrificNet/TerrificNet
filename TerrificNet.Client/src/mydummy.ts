/// <reference path="../typings/jquery/jquery.d.ts" />

namespace Gugus {
    export class MyDummy {
        doSomething(): string {
            $("#gugus").add($("<h1 />"));

            return "hallo";
        }
    }
}