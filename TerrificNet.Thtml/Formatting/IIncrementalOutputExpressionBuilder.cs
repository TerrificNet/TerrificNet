using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Formatting
{
	public interface IIncrementalOutputExpressionBuilder : IOutputExpressionBuilder
	{
		void EnterScope(IExpressionBuilder expressionBuilder, IBinding idBinding);
		void LeaveScope(IExpressionBuilder expressionBuilder);
	}
}