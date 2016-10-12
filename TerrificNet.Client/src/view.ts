import * as vdom from "virtual-dom";

export class View {
   private tree: vdom.VTree;
   private currentNode: HTMLElement;

   constructor(node: HTMLElement, tree: vdom.VTree) {
      this.currentNode = node;
      this.tree = tree;
   }

   get node(): HTMLElement {
      return this.currentNode;
   }

   update(treeCurrent: vdom.VNode) {
      const patches = vdom.diff(this.tree, treeCurrent.children[0]);
      this.tree = treeCurrent.children[0];
      this.currentNode = (vdom.patch(this.currentNode, patches) as HTMLElement);
   }

   updateAsync(treePromise: Promise<vdom.VNode>): Promise<this> {
      return treePromise.then((treeResult: vdom.VNode) => {
         this.update(treeResult);
         return this;
      });
   }

   static createFromVDom(treePromise: Promise<vdom.VNode>): Promise<View> {
      return treePromise.then((treeResult: vdom.VNode) => {
         var element = vdom.create(treeResult.children[0] as vdom.VNode) as HTMLElement;

         return new View(element, treeResult.children[0]);
      });
   }
}
