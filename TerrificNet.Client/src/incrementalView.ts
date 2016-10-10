import * as idom from "incremental-dom";

export class IncrementalView {
   private rootElement: Node;

   constructor(rootElement: Node) {
      this.rootElement = rootElement;
   }

   execute(data: any): void {
      idom.patch(this.rootElement, () => this.render(data));
   }

   executeFunc(renderFunc: (o: Function, c: Function, t: Function, v: Function, e: Function, s: Function, a: Function) => void) {
      idom.patch(this.rootElement, () => renderFunc(idom.elementOpen, idom.elementClose, idom.text, idom.elementVoid, idom.elementOpenEnd, idom.elementOpenStart, idom.attr));
   }

   executeFuncFromTemplate(template: string): any {
      const render = new Function(`return function(o, c, t, v, e, s, a) { ${template} }`)();
      this.executeFunc(render);
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