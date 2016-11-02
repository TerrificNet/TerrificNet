using System;
using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class AggregatedRenderingScopeInterceptor : IRenderingScopeInterceptor
	{
		private readonly IList<IRenderingScopeInterceptor> _scopeInterceptors;
		private Action<IRenderingScope, IExpressionBuilder, Action> _proceedingAction;

		public AggregatedRenderingScopeInterceptor(IEnumerable<IRenderingScopeInterceptor> interceptors)
		{
			_scopeInterceptors = interceptors.Reverse().ToList();
		}

		public void Intercept(IRenderingScope renderingScope, IExpressionBuilder expressionBuilder, Action action)
		{
			if (_proceedingAction == null)
			{
				_proceedingAction = (r, i, a) => a();
				foreach (var interceptor in _scopeInterceptors)
				{
					var proceedingAction = _proceedingAction;
					_proceedingAction = (r, e, a) => interceptor.Intercept(r, e, () => proceedingAction(r, e, a));
				}
			}

			_proceedingAction(renderingScope, expressionBuilder, action);
		}
	}
}