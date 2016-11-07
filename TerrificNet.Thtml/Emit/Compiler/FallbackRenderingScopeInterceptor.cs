using System;
using System.Linq;
using TerrificNet.Thtml.Formatting;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class FallbackRenderingScopeInterceptor : IRenderingScopeInterceptor
	{
		private readonly IBindingSupport _bindingSupport;
		private readonly IExpressionBuilder _fallbackExpressionBuilder;
		private readonly IIncrementalOutputExpressionBuilder _incrementalOutputExpressionBuilder;

		public FallbackRenderingScopeInterceptor(IBindingSupport bindingSupport, IExpressionBuilder fallbackExpressionBuilder, IIncrementalOutputExpressionBuilder incrementalOutputExpressionBuilder)
		{
			_bindingSupport = bindingSupport;
			_fallbackExpressionBuilder = fallbackExpressionBuilder;
			_incrementalOutputExpressionBuilder = incrementalOutputExpressionBuilder;
		}

		public void Intercept(IRenderingScope renderingScope, IExpressionBuilder expressionBuilder, Action action)
		{
			if (renderingScope.GetBindings().All(b => _bindingSupport.SupportsBinding(b)))
				action();
			else
			{
				var scopeParameters = new ScopeParameters(_fallbackExpressionBuilder, NullRenderingScopeInterceptor.Default);
				while (renderingScope.Id == null && renderingScope.Parent != null)
				{
					renderingScope = renderingScope.Parent;
				}
				if (renderingScope.Id == null)
					throw new NotSupportedException("Can't use the fallback rendering, because no id is defined");

				_incrementalOutputExpressionBuilder.EnterScope(_fallbackExpressionBuilder, renderingScope.Id);
				renderingScope.Process(scopeParameters);
				_incrementalOutputExpressionBuilder.LeaveScope(_fallbackExpressionBuilder);
			}
		}
	}
}