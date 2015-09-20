using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Parsing
{
    public class CreateElement : CreateDocument
    {
        public string TagName { get; set; }
        public List<CreateAttribute> Attributes { get; set; }

        public CreateElement(string tagName) : this(tagName, Enumerable.Empty<CreateNode>())
        {
        }

        public CreateElement(string tagName, IEnumerable<CreateNode> childNodes) : base(childNodes)
        {
            TagName = tagName;
        }

        public CreateElement(string tagName, params CreateNode[] childNodes) : base(childNodes)
        {
            TagName = tagName;
        }
    }
}