using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
	public class Statement : Node
	{
		public Statement(MustacheExpression expression, params Node[] childNodes)
		{
			Expression = expression;
			ChildNodes = childNodes;
		}

		public MustacheExpression Expression { get; }
		public Node[] ChildNodes { get; }

		public override T Accept<T>(INodeVisitor<T> visitor)
		{
			return visitor.Visit(this);
		}

		protected override bool CheckIfIsFixed()
		{
			return false;
		}
	}
}