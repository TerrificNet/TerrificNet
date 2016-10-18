using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class CompilerResult
	{
		public Expression BodyExpression { get; }
		public ParameterExpression InputExpression { get; }
		public ParameterExpression RenderingContextExpression { get; }

		public CompilerResult(Expression bodyExpression, ParameterExpression inputExpression, ParameterExpression renderingContextExpression)
		{
			BodyExpression = bodyExpression;
			InputExpression = inputExpression;
			RenderingContextExpression = renderingContextExpression;
		}
	}
}