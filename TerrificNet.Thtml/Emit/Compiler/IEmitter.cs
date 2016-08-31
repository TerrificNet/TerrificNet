namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IEmitter<out TResult>
	{
		IOutputExpressionEmitter OutputExpressionEmitter { get; }

		TResult WrapResult(CompilerResult result);
	}
}