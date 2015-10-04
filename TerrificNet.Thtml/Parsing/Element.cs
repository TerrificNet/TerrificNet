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

        public Element(string tagName, IEnumerable<Node> childNodes) : base(childNodes)
        {
            TagName = tagName;
        }

        public Element(string tagName, params Node[] childNodes) : base(childNodes)
        {
            TagName = tagName;
        }

        public Element(string tagName, IEnumerable<Node> childNodes, IEnumerable<ElementPart> attributes) : this(tagName, childNodes)
        {
            Attributes = attributes.ToList();
        }

        public Element(string tagName, IEnumerable<ElementPart> attributes) : this(tagName)
        {
            Attributes = attributes.ToList();
        }

        public override void Accept(INodeVisitor visitor)
        {
            foreach (var child in ChildNodes)
                child.Accept(visitor);

            visitor.Visit(this);
        }
    }
}