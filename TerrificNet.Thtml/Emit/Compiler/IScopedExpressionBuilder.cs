namespace TerrificNet.Thtml.Emit.Compiler
{
	internal interface IScopedExpressionBuilder : IExpressionBuilder
	{
		void UseBinding(IBinding binding);
		void Enter();
		IBindingScope Leave();
	}
}