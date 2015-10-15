using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.VDom
{
    public class VElement : VNode
    {
        public VElement(string tagName, params VTree[] children) : this(tagName, Enumerable.Empty<VProperty>(), children)
        {
        }

        public VElement(string tagName, IEnumerable<VProperty> properties, IEnumerable<VTree> children) : base(children)
        {
            TagName = tagName;
            Properties = properties?.ToList() ?? new List<VProperty>();
        }

        public VElement(string tagName, IEnumerable<VProperty> properties, params VTree[] children) : this(tagName, properties, (IEnumerable<VTree>)children)
        {
        }

        public string TagName { get; }

        public IReadOnlyList<VProperty> Properties { get; }

        public override void Accept(IVTreeVisitor visitor)
        {
            visitor.BeginVisit(this);

            foreach (var child in Children)
                child.Accept(visitor);

            visitor.EndVisit(this);
        }
    }
}