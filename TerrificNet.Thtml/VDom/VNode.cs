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
            Children = children?.ToList() ?? new List<VTree>();
        }

        public IReadOnlyList<VTree> Children { get; }

        public override void Accept(IVTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}