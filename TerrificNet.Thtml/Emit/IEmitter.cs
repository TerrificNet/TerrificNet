using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	public interface IEmitter<out TResult>
	{
		IEmitterRunnable<TResult> Emit(Document input, IDataScopeContract dataScopeContract, IHelperBinder helperBinder);
	}
}