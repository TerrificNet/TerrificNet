namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IEmitterFactory<in TResult>
	{
		IEmitter<TResult> Create();
	}
}