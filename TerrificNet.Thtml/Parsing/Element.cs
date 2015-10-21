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

        public Element(string tagName, IEnumerable<Node> childNodes) : this(tagName, Enumerable.Empty<ElementPart>(), childNodes)
        {
        }

        public Element(string tagName, params Node[] childNodes) : this(tagName, Enumerable.Empty<ElementPart>(), childNodes)
        {
        }

        public Element(string tagName, IEnumerable<ElementPart> attributes, IEnumerable<Node> childNodes) : base(childNodes)
        {
            TagName = tagName;
            Attributes = attributes.ToList();
        }

        public Element(string tagName, IEnumerable<ElementPart> attributes, params Node[] childNodes) : base(childNodes)
        {
            TagName = tagName;
            Attributes = attributes.ToList();
        }

        public Element(string tagName, IEnumerable<ElementPart> attributes) : this(tagName, attributes, Enumerable.Empty<Node>())
        {
        }
    }
}