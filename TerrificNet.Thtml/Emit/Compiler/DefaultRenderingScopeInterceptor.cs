using System;
using System.Linq;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class DefaultRenderingScopeInterceptor : IRenderingScopeInterceptor
	{
		private readonly IBindingSupport _bindingSupport;

		public DefaultRenderingScopeInterceptor(IBindingSupport bindingSupport)
		{
			_bindingSupport = bindingSupport;
		}

		public void Intercept(IRenderingScope renderingScope, IExpressionBuilder expressionBuilder, Action action)
		{
			if (!renderingScope.GetBindings().All(_bindingSupport.SupportsBinding))
				return;

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