using System;
using System.IO;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	public interface IEmitter<out TResult, TEmit, TConfig>
	{
		IEmitterRunnable<TResult> Emit(Document input, IDataScopeContract dataScopeContract, IHelperBinder helperBinder);
	}
}