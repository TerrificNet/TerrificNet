using System.Linq.Expressions;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IEmitter
	{
		IOutputExpressionBuilder ExpressionBuilder { get; }

		LambdaExpression CreateExpression(CompilerResult result);

		ParameterExpression RendererExpression { get; }

		IViewTemplate CreateTemplate(CompilerResult result);
	}

	public interface IEmitter<in TRenderer> : IEmitter
	{
		IViewTemplate<TRenderer> WrapResult(CompilerResult result);
	}
}