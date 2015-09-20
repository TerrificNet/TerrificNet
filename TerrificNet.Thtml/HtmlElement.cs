using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml
{
    public class HtmlElement : HtmlDocument
    {
        public string TagName { get; set; }

        public HtmlElement(string tagName) : this(tagName, Enumerable.Empty<HtmlNode>())
        {
        }

        public HtmlElement(string tagName, IEnumerable<HtmlNode> childNodes) : base(childNodes)
        {
            TagName = tagName;
        }

        public HtmlElement(string tagName, params HtmlNode[] childNodes) : base(childNodes)
        {
            TagName = tagName;
        }
    }
}