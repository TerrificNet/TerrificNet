using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Emit
{
	public abstract class HelperBinderResult<TEmit, TConfig>
	{
		public abstract TEmit CreateEmitter(TConfig config, TEmit children, IHelperBinder<TEmit, TConfig> helperBinder, IDataScopeContract scopeContract);
	}
}