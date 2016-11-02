namespace TerrificNet.Thtml.Emit.Compiler
{
	public class ScopeParameters
	{
		public ScopeParameters(IExpressionBuilder expressionBuilder, IRenderingScopeInterceptor interceptor)
		{
			ExpressionBuilder = expressionBuilder;
			Interceptor = interceptor;
		}

		public IExpressionBuilder ExpressionBuilder { get; }

		public IRenderingScopeInterceptor Interceptor { get; }
	}
}