using System.Collections.Generic;
using System.Linq.Expressions;
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

		public Expression ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementOpenStart(null));
			var elementOpenStart = Expression.Call(InstanceExpression, method, Expression.Constant(tagName));

			if (staticProperties.Count > 0)
			{
				var expressions = new List<Expression> { elementOpenStart };
				foreach (var property in staticProperties)
				{
					expressions.Add(PropertyStart(property.Key));
					expressions.Add(Value(Expression.Constant(property.Value)));
					expressions.Add(PropertyEnd());
				}

				return Expression.Block(expressions);
			}

			return elementOpenStart;
		}

		public Expression ElementOpenEnd()
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementOpenEnd());
			return Expression.Call(InstanceExpression, method);
		}

		public Expression ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			if (staticProperties.Count > 0)
			{
				return Expression.Block(ElementOpenStart(tagName, staticProperties), ElementOpenEnd());
			}

			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementOpen(null));
			return Expression.Call(InstanceExpression, method, Expression.Constant(tagName));
		}

		public Expression ElementClose(string tagName)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementClose());
			return Expression.Call(InstanceExpression, method);
		}

		public Expression PropertyStart(string propertyName)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.PropertyStart(null));
			return Expression.Call(InstanceExpression, method, Expression.Constant(propertyName));
		}

		public Expression PropertyEnd()
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.PropertyEnd());
			return Expression.Call(InstanceExpression, method);
		}

		public Expression Value(Expression value)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.Value(null));
			return Expression.Call(InstanceExpression, method, value);
		}
	}
}