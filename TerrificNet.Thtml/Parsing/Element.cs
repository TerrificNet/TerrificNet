using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Parsing
{
    public class Element : Document
    {
        public string TagName { get; }
        public IReadOnlyList<ElementPart> Attributes { get; }

        public Element(string tagName) : this(tagName, Enumerable.Empty<Node>())
        {
        }

        public Element(string tagName, IEnumerable<Node> childNodes) : this(tagName, childNodes, Enumerable.Empty<ElementPart>())
        {
        }

        public Element(string tagName, params Node[] childNodes) : this(tagName, childNodes, Enumerable.Empty<ElementPart>())
        {
        }

        public Element(string tagName, IEnumerable<Node> childNodes, IEnumerable<ElementPart> attributes) : base(childNodes)
        {
            TagName = tagName;
            Attributes = attributes.ToList();
        }

        public Element(string tagName, IEnumerable<ElementPart> attributes) : this(tagName, Enumerable.Empty<Node>(), attributes)
        {
        }

        public override void Accept(INodeVisitor visitor)
        {
            if (!visitor.BeforeVisit(this))
                return;

            foreach (var attribute in Attributes)
                attribute.Accept(visitor);

            foreach (var child in ChildNodes)
                child.Accept(visitor);

            visitor.AfterVisit(this);
        }
    }
}