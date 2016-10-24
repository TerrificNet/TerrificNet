using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IExpressionBuilder
	{
		void Add(Expression expression);

		void DefineVariable(ParameterExpression expression);
	}
}