using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Emit.Compiler
{
	class EmitIlExpressionVisitor : IExpressionVisitor
	{
		public void Visit(CallHelperExpression callHelperExpression)
		{

		}

		public bool BeforeVisit(ConditionalExpression conditionalExpression)
		{
			return true;
		}

		public void AfterVisit(ConditionalExpression conditionalExpression)
		{
			
		}

		public bool BeforeVisit(IterationExpression iterationExpression)
		{
			return true;
		}

		public void AfterVisit(IterationExpression iterationExpression)
		{
		}

		public bool BeforeVisit(MemberExpression memberExpression)
		{
			return true;
		}

		public void AfterVisit(MemberExpression memberExpression)
		{
		}

		public bool BeforeVisit(UnconvertedExpression unconvertedExpression)
		{
			return true;
		}

		public void AfterVisit(UnconvertedExpression unconvertedExpression)
		{
		}
	}
}