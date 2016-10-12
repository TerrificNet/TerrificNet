import * as thtml from "terrific-net"

export class TodoComponent {

   private view: thtml.IncrementalView;

   constructor(node: Node) {
      this.view = new thtml.IncrementalView(document.body);
   }

   public add(description: string): void {
      thtml.post<string>('/todo', description, false).then(template => {
         this.view.executeFuncFromTemplate(template);
      });
   }
}