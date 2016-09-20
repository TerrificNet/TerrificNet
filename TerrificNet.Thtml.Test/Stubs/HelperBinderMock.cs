using System.Collections.Generic;
using LightMock;
using TerrificNet.Thtml.Emit;

namespace TerrificNet.Thtml.Test.Stubs
{
	internal class HelperBinderMock : IHelperBinder
	{
		private readonly IInvocationContext<IHelperBinder> _invocationContext;

		public HelperBinderMock(IInvocationContext<IHelperBinder> invocationContext)
		{
			_invocationContext = invocationContext;
		}

		public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
		{
			return _invocationContext.Invoke(f => f.FindByName(helper, arguments));
		}
	}
}