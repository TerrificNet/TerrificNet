using System.Linq.Expressions;
using LightMock;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Test
{
	internal class HelperBinderResultMock<TEmit, TConfig> : HelperBinderResult<TEmit, TConfig>
	{
		private readonly IInvocationContext<HelperBinderResult<TEmit, TConfig>> _invocationContext;

		public HelperBinderResultMock(IInvocationContext<HelperBinderResult<TEmit, TConfig>> invocationContext)
		{
			_invocationContext = invocationContext;
		}

		public override TEmit CreateEmitter(HelperParameters helperParameters, TEmit children)
		{
			return _invocationContext.Invoke(f => f.CreateEmitter(helperParameters, children));
		}
	}
}