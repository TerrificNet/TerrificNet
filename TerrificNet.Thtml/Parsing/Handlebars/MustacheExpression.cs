namespace TerrificNet.Thtml.Parsing.Handlebars
{
	public abstract class MustacheExpression : SyntaxNode
	{
		public abstract T Accept<T>(INodeVisitor<T> visitor);

		public abstract void Accept(INodeVisitor visitor);

		protected override bool CheckIfIsFixed()
		{
			return false;
		}
	}
}