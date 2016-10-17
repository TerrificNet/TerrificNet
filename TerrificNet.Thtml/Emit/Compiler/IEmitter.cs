using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IEmitter
	{
		IOutputExpressionBuilder ExpressionBuilder { get; }

		ParameterExpression RendererExpression { get; }
	}
}