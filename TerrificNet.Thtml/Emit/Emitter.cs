using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    public class Emitter
    {
        public Func<VTree> Emit(Document input)
        {
            var visitor = new EmitNodeVisitor();
            input.Accept(visitor);
            return () => visitor.Document;
        }
    }

    public class EmitNodeVisitor : NodeVisitor
    {
        private readonly List<VTree> _elements = new List<VTree>(); 

        public VNode Document { get; private set; }

        public override void Visit(Element element)
        {
            var children = _elements.ToList();
            _elements.Clear();
            _elements.Add(new VElement(element.TagName, children));
        }

        public override void Visit(TextNode textNode)
        {
            _elements.Add(new VText(textNode.Text));
        }

        public override void Visit(Document document)
        {
            this.Document = new VNode(_elements);
        }
    }
}