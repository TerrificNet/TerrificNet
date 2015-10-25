using System.Collections.Generic;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public abstract class MustacheExpression : SyntaxNode
    {
	    public abstract void Accept(INodeVisitor visitor);
    }
}