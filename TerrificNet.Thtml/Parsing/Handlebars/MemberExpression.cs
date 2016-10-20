namespace TerrificNet.Thtml.Parsing.Handlebars
{
	public class MemberExpression : AccessExpression
	{
		public string Name { get; }

		public MemberExpression(string name, MemberExpression subExpression = null) : base(subExpression)
		{
			Name = name;
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