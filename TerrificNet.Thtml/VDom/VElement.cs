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
            PropertyList = properties?.ToList() ?? new List<VProperty>();
        }

        public VElement(string tagName, IEnumerable<VProperty> properties, params VTree[] children) : this(tagName, properties, (IEnumerable<VTree>)children)
        {
        }

        public string TagName { get; }

        internal IReadOnlyList<VProperty> PropertyList { get; }

        public Dictionary<string, Dictionary<string, object>> Properties
        {
            get
            {
                return new Dictionary<string, Dictionary<string, object>>
                {
                    { "attributes", PropertyList.ToDictionary(d => d.Name, d => d.Value.GetValue()) }
                };
            }
        }

        public override void Accept(IVTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}