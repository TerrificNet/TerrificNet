using System;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IRenderingScopeInterceptor
	{
		void Intercept(IRenderingScope renderingScope, IExpressionBuilder expressionBuilder, Action action);
	}
}