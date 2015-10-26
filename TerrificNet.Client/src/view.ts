/// <reference path="../typings/tsd.d.ts" />
/// <reference path="../typings_custom/virtual-dom.d.ts" />

import createElement = require("virtual-dom/create-element");
import diff = require("virtual-dom/diff");
import patch = require("virtual-dom/patch");

export class View {
    private tree: VTree;
    private _node: HTMLElement;

    constructor(node: HTMLElement, tree: VTree) {
        this._node = node;
        this.tree = tree;
    }

    get node(): HTMLElement {
        return this._node;
    }

    update(treeCurrent: VNode) {
        var patches = diff(this.tree, treeCurrent.children[0]);
        this.tree = treeCurrent.children[0];
        this._node = <HTMLElement>patch(this._node, patches);
    }

    updateAsync(treePromise: Promise<VNode>): Promise<void> {
        return treePromise.then((treeResult: VNode) => this.update(treeResult));
    }

    static createFromVDom(treePromise: Promise<VNode>): Promise<View> {
        return treePromise.then((treeResult: VNode) => {
            var element = <HTMLElement>createElement(treeResult.children[0]);

            return new View(element, treeResult.children[0]);
        });
    }
}
