import * as idom from "incremental-dom";

export class IncrementalView {
   private rootElement: Node;

   constructor(rootElement: Node) {
      this.rootElement = rootElement;
   }

   execute(data: any): void {
      idom.patch(this.rootElement, () => this.render(data));
   }

   executeFunc(renderFunc: (o: Function, c: Function, t: Function, v: Function, e: Function, s: Function, a: Function, $scope: any) => void, scope: any = null): void {
      idom.patch(this.rootElement,
         () => 
            renderFunc(idom.elementOpen,
               idom.elementClose,
               idom.text,
               idom.elementVoid,
               idom.elementOpenEnd,
               idom.elementOpenStart,
               idom.attr,
               scope
            )
      );
   }

   executeFuncFromTemplate(template: string, scope: any = null): any {
      const render = new Function(`return function(o, c, t, v, e, s, a, $scope) { ${template} }`)();
      this.executeFunc(render, scope);
   }

   private render(data: any): void {
      const o = idom.elementOpen;
      const c = idom.elementClose;
      const t = idom.text;

      o("div", "", ["class", "mod"]);
      o("a", "", ["href", data.url]);
      t(data.content);
      c("a");
      c("div");
   }
}