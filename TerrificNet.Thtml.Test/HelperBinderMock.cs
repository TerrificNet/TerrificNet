using System.Collections.Generic;
using LightMock;
using TerrificNet.Thtml.Emit;

namespace TerrificNet.Thtml.Test
{
	internal class HelperBinderMock<TEmit, TConfig> : IHelperBinder<TEmit, TConfig>
	{
		private readonly IInvocationContext<IHelperBinder<TEmit, TConfig>> _invocationContext;

		public HelperBinderMock(IInvocationContext<IHelperBinder<TEmit, TConfig>> invocationContext)
		{
			_invocationContext = invocationContext;
		}

		public HelperBinderResult<TEmit, TConfig> FindByName(string helper, IDictionary<string, string> arguments)
		{
			return _invocationContext.Invoke(f => f.FindByName(helper, arguments));
		}
	}
}