using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Emit
{
	public abstract class HelperBinderResult<TEmit, TConfig>
	{
		public abstract TEmit CreateEmitter(Handler handler, TEmit children, IHelperBinder<TEmit, TConfig> helperBinder, IDataScopeContract scopeContract);
	}
}