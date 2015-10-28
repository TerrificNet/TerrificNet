using System.Collections.Generic;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class UnconvertedExpression : MustacheExpression
    {
        public MustacheExpression Expression { get; }

        public UnconvertedExpression(MustacheExpression expression)
        {
            Expression = expression;
        }

        public override T Accept<T>(INodeVisitor<T> visitor)
        {
	        return visitor.Visit(this);
        }
    }
}