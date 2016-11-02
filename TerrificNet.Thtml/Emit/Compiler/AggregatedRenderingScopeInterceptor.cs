using System;
using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class AggregatedRenderingScopeInterceptor : IRenderingScopeInterceptor
	{
		private readonly IList<IRenderingScopeInterceptor> _scopeInterceptors;

		public AggregatedRenderingScopeInterceptor(IEnumerable<IRenderingScopeInterceptor> interceptors)
		{
			_scopeInterceptors = interceptors.Reverse().ToList();
		}

		public void Intercept(IRenderingScope renderingScope, IExpressionBuilder expressionBuilder, Action action)
		{
			var finalAction = action;
			foreach (var interceptor in _scopeInterceptors)
			{
				var proceedingAction = finalAction;
				finalAction = () => interceptor.Intercept(renderingScope, expressionBuilder, proceedingAction);
			}

			finalAction();
		}
	}
}