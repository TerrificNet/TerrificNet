using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml
{
    public class HtmlDocument : HtmlNode
    {
        public HtmlDocument()
        {
            ChildNodes = new List<HtmlNode>();
        }

        public HtmlDocument(IEnumerable<HtmlNode> childNodes)
        {
            ChildNodes = childNodes.ToList();
        }

        public HtmlDocument(params HtmlNode[] childNodes) 
            : this((IEnumerable<HtmlNode>)childNodes)
        {
        }
        public IReadOnlyList<HtmlNode> ChildNodes { get; }

    }
}