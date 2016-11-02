using System;
using System.Linq;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class RequiredRenderingInterceptor : IRenderingScopeInterceptor
	{
		public void Intercept(IRenderingScope renderingScope, IExpressionBuilder expressionBuilder, Action action)
		{
			if (!renderingScope.IsEmpty() && renderingScope.GetBindings().Any())
			{
				var testExpression = Expression.Constant(true);

				expressionBuilder.IfThen(testExpression, action);
			}
			else
			{
				action();
			}
		}
	}
}