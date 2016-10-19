using System.Linq.Expressions;

namespace TerrificNet.Thtml.Formatting
{
	public interface IOutputExpressionBuilderFactory
	{
		IOutputExpressionBuilder CreateExpressionBuilder(Expression parameter);
	}
}