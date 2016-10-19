using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IOutputExpressionBuilderFactory
	{
		IOutputExpressionBuilder CreateExpressionBuilder(Expression parameter);
	}
}