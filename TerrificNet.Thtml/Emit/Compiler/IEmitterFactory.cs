namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IEmitterFactory<out TResult>
	{
		IEmitter<TResult> Create();
	}
}