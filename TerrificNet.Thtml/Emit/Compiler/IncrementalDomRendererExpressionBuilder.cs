using System.Linq.Expressions;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class IncrementalDomRendererExpressionBuilder
	{
		private readonly Expression _parameterExpression;

		public IncrementalDomRendererExpressionBuilder(Expression parameterExpression)
		{
			_parameterExpression = parameterExpression;
		}

		public MethodCallExpression Text(Expression callExpression)
		{
			return Expression.Call(_parameterExpression, ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.Text(null)), callExpression);
		}

		public MethodCallExpression ElementVoid(string tagName, Expression staticAttributeList, Expression attributeList)
		{
			return Expression.Call(_parameterExpression, ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.ElementVoid(null, null, null, null)), Expression.Constant(tagName), Expression.Constant(null, typeof(string)), staticAttributeList, attributeList);
		}

		public MethodCallExpression ElementClose(string tagName)
		{
			return Expression.Call(_parameterExpression, ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.ElementClose(null)), Expression.Constant(tagName));
		}

		public MethodCallExpression ElementOpen(string tagName, Expression staticAttributeList, Expression attributeList)
		{
			return Expression.Call(_parameterExpression, ExpressionHelper.GetMethodInfo<IIncrementalDomRenderer>(r => r.ElementOpen(null, null, null, null)), Expression.Constant(tagName), Expression.Constant(null, typeof(string)), staticAttributeList, attributeList);
		}
	}
}