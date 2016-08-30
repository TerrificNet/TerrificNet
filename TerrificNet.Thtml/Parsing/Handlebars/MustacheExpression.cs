namespace TerrificNet.Thtml.Parsing.Handlebars
{
	public abstract class MustacheExpression : SyntaxNode
	{
		public abstract T Accept<T>(INodeVisitor<T> visitor);
	}
}