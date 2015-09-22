using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Parsing
{
    public class Element : Document
    {
        public string TagName { get; }
        public IReadOnlyList<Attribute> Attributes { get; }

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

        public Element(string tagName, IEnumerable<Node> childNodes, IEnumerable<Attribute> attributes) : this(tagName, childNodes)
        {
            Attributes = attributes.ToList();
        }

        public Element(string tagName, IEnumerable<Attribute> attributes) : this(tagName)
        {
            Attributes = attributes.ToList();
        }
    }
}