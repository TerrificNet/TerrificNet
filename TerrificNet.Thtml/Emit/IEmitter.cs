using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	public interface IEmitter<out TResult>
	{
		IRunnable<TResult> Emit(Document input, IDataScopeContract dataScopeContract, IHelperBinder helperBinder);
	}
}