using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class CompilerResult
	{
		public Expression BodyExpression { get; }
		public ParameterExpression InputExpression { get; }

		public CompilerResult(Expression bodyExpression, ParameterExpression inputExpression)
		{
			BodyExpression = bodyExpression;
			InputExpression = inputExpression;
		}
	}
}