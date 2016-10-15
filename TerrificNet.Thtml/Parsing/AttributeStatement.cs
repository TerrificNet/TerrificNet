using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
	public class AttributeStatement : ElementPart
	{
		public MustacheExpression Expression { get; }
		public AttributeNode[] ChildNodes { get; }

		public AttributeStatement(MustacheExpression expression, params AttributeNode[] childNodes)
		{
			Expression = expression;
			ChildNodes = childNodes;
		}

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