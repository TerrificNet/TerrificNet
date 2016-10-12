using System.Linq.Expressions;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class VDomBuilderExpression
	{
		private readonly Expression _instance;

		public VDomBuilderExpression(Expression instance)
		{
			_instance = instance;
		}

		public Expression ElementOpenStart(string tagName)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementOpenStart(null));
			return Expression.Call(_instance, method, Expression.Constant(tagName));
		}

		public Expression ElementOpenEnd()
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementOpenEnd());
			return Expression.Call(_instance, method);
		}

		public Expression ElementOpen(string tagName)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementOpen(null));
			return Expression.Call(_instance, method, Expression.Constant(tagName));
		}

		public Expression ElementClose(string tagName)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.ElementClose());
			return Expression.Call(_instance, method);
		}

		public Expression PropertyStart(string propertyName)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.PropertyStart(null));
			return Expression.Call(_instance, method, Expression.Constant(propertyName));
		}

		public Expression PropertyEnd()
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.PropertyEnd());
			return Expression.Call(_instance, method);
		}

		public Expression Value(Expression value)
		{
			var method = ExpressionHelper.GetMethodInfo<IVDomBuilder>(e => e.Value(null));
			return Expression.Call(_instance, method, value);
		}
	}
}