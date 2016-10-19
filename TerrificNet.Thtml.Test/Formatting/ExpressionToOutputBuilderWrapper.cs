using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TerrificNet.Thtml.Formatting;

namespace TerrificNet.Thtml.Test.Formatting
{
	internal class ExpressionToOutputBuilderWrapper : IOutputBuilder
	{
		private readonly IOutputExpressionBuilder _builder;

		public ExpressionToOutputBuilderWrapper(IOutputExpressionBuilder builder)
		{
			_builder = builder;
		}

		public void ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			ExecuteExpression(_builder.ElementOpenStart(tagName, staticProperties));
		}

		public void ElementOpenEnd()
		{
			ExecuteExpression(_builder.ElementOpenEnd());
		}

		public void ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			ExecuteExpression(_builder.ElementOpen(tagName, staticProperties));
		}

		public void ElementClose(string tagName)
		{
			ExecuteExpression(_builder.ElementClose(tagName));
		}

		public void PropertyStart(string propertyName)
		{
			ExecuteExpression(_builder.PropertyStart(propertyName));
		}

		public void PropertyEnd()
		{
			ExecuteExpression(_builder.PropertyEnd());
		}

		public void Value(string value)
		{
			ExecuteExpression(_builder.Value(Expression.Constant(value)));
		}

		private static void ExecuteExpression(Expression expression)
		{
			var func = Expression.Lambda<Action>(expression).Compile();
			func();
		}
	}
}