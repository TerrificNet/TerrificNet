using System;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class OutputExpressionBuilderFactory<TRenderer> : IOutputExpressionBuilderFactory
	{
		private readonly IOutputExpressionBuilder _expressionBuilder;

		public OutputExpressionBuilderFactory(Func<ParameterExpression, IOutputExpressionBuilder> builderFactory)
		{
			_expressionBuilder = builderFactory(Expression.Parameter(typeof(TRenderer)));
		}

		public IOutputExpressionBuilder CreateExpressionBuilder()
		{
			return _expressionBuilder;
		}
	}
}
