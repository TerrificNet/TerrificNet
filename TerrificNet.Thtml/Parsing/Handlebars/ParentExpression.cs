namespace TerrificNet.Thtml.Parsing.Handlebars
{
	public class ParentExpression : AccessExpression
	{
		public ParentExpression(AccessExpression subExpression = null) : base(subExpression)
		{
		}

		public override T Accept<T>(INodeVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}
}