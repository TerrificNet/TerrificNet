namespace TerrificNet.Thtml.Parsing.Handlebars
{
	public class ConditionalExpression : MustacheExpression
	{
		public MustacheExpression Expression { get; }

		public ConditionalExpression(MustacheExpression expression)
		{
			Expression = expression;
		}

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