/// <reference path="../typings/jquery/jquery.d.ts" />

//import dummy = require("mydummy");

export module Gugus {
    export class MyDummy2 {
        doSomething(): string {
            $("#gugus").add($("<h1 />"));

            return "hallo";
        }
    }
}