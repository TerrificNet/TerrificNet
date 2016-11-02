using System;
using System.Linq;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class OnlySupportedBindingScopeInterceptor : IRenderingScopeInterceptor
	{
		private readonly IBindingSupport _bindingSupport;

		public OnlySupportedBindingScopeInterceptor(IBindingSupport bindingSupport)
		{
			_bindingSupport = bindingSupport;
		}

		public void Intercept(IRenderingScope renderingScope, IExpressionBuilder expressionBuilder, Action action)
		{
			if (!renderingScope.GetBindings().All(_bindingSupport.SupportsBinding))
				return;

			action();
		}
	}
}