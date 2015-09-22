using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Parsing
{
    public class Document : Node
    {
        public Document()
        {
            ChildNodes = new List<Node>();
        }

        public Document(IEnumerable<Node> childNodes)
        {
            ChildNodes = childNodes.ToList();
        }

        public Document(params Node[] childNodes) 
            : this((IEnumerable<Node>)childNodes)
        {
        }

        public IReadOnlyList<Node> ChildNodes { get; }

    }
}