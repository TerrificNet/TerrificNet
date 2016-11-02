using System;
using System.Linq;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class FallbackRenderingScopeInterceptor : IRenderingScopeInterceptor
	{
		private readonly IBindingSupport _bindingSupport;
		private readonly IExpressionBuilder _fallbackExpressionBuilder;

		public FallbackRenderingScopeInterceptor(IBindingSupport bindingSupport, IExpressionBuilder fallbackExpressionBuilder)
		{
			_bindingSupport = bindingSupport;
			_fallbackExpressionBuilder = fallbackExpressionBuilder;
		}

		public void Intercept(IRenderingScope renderingScope, IExpressionBuilder expressionBuilder, Action action)
		{
			if (renderingScope.GetBindings().All(b => _bindingSupport.SupportsBinding(b)))
				action();
			else
				renderingScope.Process(new ScopeParameters(_fallbackExpressionBuilder, NullRenderingScopeInterceptor.Default));
		}
	}
}