using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.VDom
{
    public class VNode : VTree
    {
        public VNode(params VTree[] children) : this((IEnumerable<VTree>)children)
        {
        }

        public VNode(IEnumerable<VTree> children)
        {
            Children = children?.Where(c => c != null).ToList() ?? new List<VTree>();
        }

        public int Count => Children.Count;

        public IReadOnlyList<VTree> Children { get; }

        public override void Accept(IVTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string Type => "VirtualNode";
    }
}