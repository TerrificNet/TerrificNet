namespace TerrificNet.Thtml.Parsing
{
	public abstract class HtmlNode : SyntaxNode
	{
		public abstract T Accept<T>(INodeVisitor<T> visitor);
	}
}