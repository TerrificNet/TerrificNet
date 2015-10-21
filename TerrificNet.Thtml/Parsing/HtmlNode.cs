namespace TerrificNet.Thtml.Parsing
{
	public abstract class HtmlNode : SyntaxNode
	{
		public abstract void Accept(INodeVisitor visitor);
	}
}