using System.Collections.Generic;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Formatting.VDom
{
	internal class VDomOutputExpressionBuilder : IOutputExpressionBuilder
	{
		public VDomOutputExpressionBuilder(Expression instance)
		{
			InstanceExpression = instance;
		}

		public Expression InstanceExpression { get; }

		public void ElementOpenStart(IExpressionBuilder expressionBuilder, string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementOpenStart(null));
			expressionBuilder.Add(Expression.Call(InstanceExpression, method, Expression.Constant(tagName)));

			if (staticProperties != null && staticProperties.Count > 0)
			{
				foreach (var property in staticProperties)
				{
					PropertyStart(expressionBuilder, property.Key);
					Value(expressionBuilder, Expression.Constant(property.Value));
					PropertyEnd(expressionBuilder);
				}
			}
		}

		public void ElementOpenEnd(IExpressionBuilder expressionBuilder)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementOpenEnd());
			expressionBuilder.Add(Expression.Call(InstanceExpression, method));
		}

		public void ElementOpen(IExpressionBuilder expressionBuilder, string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			if (staticProperties.Count > 0)
			{
				ElementOpenStart(expressionBuilder, tagName, staticProperties);
				ElementOpenEnd(expressionBuilder);

				return;
			}

			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementOpen(null));
			expressionBuilder.Add(Expression.Call(InstanceExpression, method, Expression.Constant(tagName)));
		}

		public void ElementClose(IExpressionBuilder expressionBuilder, string tagName)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementClose());
			expressionBuilder.Add(Expression.Call(InstanceExpression, method));
		}

		public void PropertyStart(IExpressionBuilder expressionBuilder, string propertyName)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.PropertyStart(null));
			expressionBuilder.Add(Expression.Call(InstanceExpression, method, Expression.Constant(propertyName)));
		}

		public void PropertyEnd(IExpressionBuilder expressionBuilder)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.PropertyEnd());
			expressionBuilder.Add(Expression.Call(InstanceExpression, method));
		}

		public void Value(IExpressionBuilder expressionBuilder, IBinding valueBinding)
		{
			Expression expression;
			if (!valueBinding.TryGetExpression(out expression))
				return;

			Value(expressionBuilder, expression);
		}

		public void Text(IExpressionBuilder expressionBuilder, string text)
		{
			Value(expressionBuilder, Expression.Constant(text));
		}

		private void Value(IExpressionBuilder expressionBuilder, Expression value)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.Value(null));
			expressionBuilder.Add(Expression.Call(InstanceExpression, method, value));
		}
	}
}