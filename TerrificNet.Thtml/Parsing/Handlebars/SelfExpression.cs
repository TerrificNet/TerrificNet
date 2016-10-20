namespace TerrificNet.Thtml.Parsing.Handlebars
{
	public class SelfExpression : MustacheExpression
	{
		public override T Accept<T>(INodeVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}