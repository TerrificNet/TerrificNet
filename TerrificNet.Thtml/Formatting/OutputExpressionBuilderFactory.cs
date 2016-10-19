using System;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Formatting
{
	public class OutputExpressionBuilderFactory<TRenderer> : IOutputExpressionBuilderFactory
	{
		private readonly Func<Expression, IOutputExpressionBuilder> _builderFactory;

		public OutputExpressionBuilderFactory(Func<Expression, IOutputExpressionBuilder> builderFactory)
		{
			_builderFactory = builderFactory;
		}

		public IOutputExpressionBuilder CreateExpressionBuilder(Expression parameter)
		{
			return _builderFactory(Expression.ConvertChecked(parameter, typeof(TRenderer)));
		}
	}
}
