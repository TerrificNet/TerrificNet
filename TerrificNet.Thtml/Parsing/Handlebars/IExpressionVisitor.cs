using System;

namespace TerrificNet.Thtml.Parsing.Handlebars
{
	public interface IExpressionVisitor
	{
		void Visit(UnconvertedExpression unconvertedExpression);
		void Visit(IterationExpression iterationExpression);
		void Visit(ConditionalExpression conditionalExpression);
		void Visit(CallHelperExpression callHelperExpression);
		void Visit(MemberExpression memberExpression);
	}
}