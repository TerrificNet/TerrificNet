using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Compiler;
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
			ExecuteExpression(b => _builder.ElementOpenStart(b, tagName, staticProperties));
		}

		public void ElementOpenEnd()
		{
			ExecuteExpression(b => _builder.ElementOpenEnd(b));
		}

		public void ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			ExecuteExpression(b => _builder.ElementOpen(b, tagName, staticProperties));
		}

		public void ElementClose(string tagName)
		{
			ExecuteExpression(b => _builder.ElementClose(b, tagName));
		}

		public void PropertyStart(string propertyName)
		{
			ExecuteExpression(b => _builder.PropertyStart(b, propertyName));
		}

		public void PropertyEnd()
		{
			ExecuteExpression(b => _builder.PropertyEnd(b));
		}

		public void Value(string value)
		{
			ExecuteExpression(b => _builder.Value(b, new ConstantBinding(value)));
		}

		private static void ExecuteExpression(Action<IExpressionBuilder> buildAction)
		{
			var builder = new ExpressionBuilder();
			buildAction(builder);

			var func = Expression.Lambda<Action>(builder.BuildExpression()).Compile();
			func();
		}
	}
}