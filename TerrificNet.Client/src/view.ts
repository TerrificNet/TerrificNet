/// <reference path="../typings/tsd.d.ts" />
/// <reference path="../typings_custom/virtual-dom.d.ts" />

import createElement = require("virtual-dom/create-element");
import diff = require("virtual-dom/diff");
import patch = require("virtual-dom/patch");

export class View {
   private tree: VTree;
   private currentNode: HTMLElement;

   constructor(node: HTMLElement, tree: VTree) {
      this.currentNode = node;
      this.tree = tree;
   }

   get node(): HTMLElement {
      return this.currentNode;
   }

   update(treeCurrent: VNode) {
      const patches = diff(this.tree, treeCurrent.children[0]);
      this.tree = treeCurrent.children[0];
      this.currentNode = (patch(this.currentNode, patches) as HTMLElement);
   }

   updateAsync(treePromise: Promise<VNode>): Promise<View> {
      return treePromise.then((treeResult: VNode) => {
         this.update(treeResult);
         return this;
      });
   }

   static createFromVDom(treePromise: Promise<VNode>): Promise<View> {
      return treePromise.then((treeResult: VNode) => {
         var element = createElement(treeResult.children[0]) as HTMLElement;

         return new View(element, treeResult.children[0]);
      });
   }
}
