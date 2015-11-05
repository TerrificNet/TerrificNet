using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Emit
{
	public abstract class HelperBinderResult<TEmit, TConfig>
	{
		public abstract TEmit CreateEmitter(IOutputExpressionEmitter outputExpressionEmitter, TEmit children, IHelperBinder<TEmit, TConfig> helperBinder, IDataScopeContract scopeContract);
	}
}