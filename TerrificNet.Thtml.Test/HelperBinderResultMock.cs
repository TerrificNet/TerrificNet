using System.Linq.Expressions;
using LightMock;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Test
{
	internal class HelperBinderResultMock<TEmit, TConfig> : HelperBinderResult
	{
		private readonly IInvocationContext<HelperBinderResult> _invocationContext;

		public HelperBinderResultMock(IInvocationContext<HelperBinderResult> invocationContext)
		{
			_invocationContext = invocationContext;
		}

		public override Expression CreateEmitter(HelperParameters helperParameters, Expression children)
		{
			return _invocationContext.Invoke(f => f.CreateEmitter(helperParameters, children));
		}
	}
}