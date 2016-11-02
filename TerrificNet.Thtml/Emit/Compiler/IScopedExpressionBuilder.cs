namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IScopedExpressionBuilder : IExpressionBuilder
	{
		void UseBinding(IBinding binding);
		void Enter();
		IRenderingScope Leave();
	}
}