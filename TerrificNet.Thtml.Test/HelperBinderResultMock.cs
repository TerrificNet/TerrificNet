using System.Linq.Expressions;
using LightMock;
using TerrificNet.Thtml.Emit;

namespace TerrificNet.Thtml.Test
{
	internal class HelperBinderResultMock : HelperBinderResult
	{
		private readonly IInvocationContext<HelperBinderResult> _invocationContext;

		public HelperBinderResultMock(IInvocationContext<HelperBinderResult> invocationContext)
		{
			_invocationContext = invocationContext;
		}

		public override Expression CreateExpression(HelperParameters helperParameters, Expression children)
		{
			return _invocationContext.Invoke(f => f.CreateExpression(helperParameters, children));
		}
	}
}