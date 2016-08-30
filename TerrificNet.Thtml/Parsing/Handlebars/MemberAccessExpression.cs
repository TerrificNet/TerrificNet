namespace TerrificNet.Thtml.Parsing.Handlebars
{
	public class MemberExpression : MustacheExpression
	{
		public string Name { get; }

		public MemberExpression SubExpression { get; }

		public MemberExpression(string name)
		{
			Name = name;
		}

		public MemberExpression(string name, MemberExpression subExpression)
			: this(name)
		{
			SubExpression = subExpression;
		}

		public override T Accept<T>(INodeVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}
	}
}