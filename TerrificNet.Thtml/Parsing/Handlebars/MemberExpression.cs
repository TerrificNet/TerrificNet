namespace TerrificNet.Thtml.Parsing.Handlebars
{
	public abstract class AccessExpression : MustacheExpression
	{
		protected AccessExpression(AccessExpression subExpression)
		{
			SubExpression = subExpression;
		}

		public AccessExpression SubExpression { get; }
	}

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
	}
}