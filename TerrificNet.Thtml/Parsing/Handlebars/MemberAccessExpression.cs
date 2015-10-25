using System.Collections.Generic;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class MemberExpression : MustacheExpression
    {
        public string Name { get; }

        public MustacheExpression SubExpression { get; }

        public MemberExpression(string name)
        {
            Name = name;
        }

        public MemberExpression(string name, MustacheExpression subExpression)
            : this(name)
        {
            SubExpression = subExpression;
        }

        public override void Accept(INodeVisitor visitor)
        {
			visitor.Visit(this);
        }
    }
}