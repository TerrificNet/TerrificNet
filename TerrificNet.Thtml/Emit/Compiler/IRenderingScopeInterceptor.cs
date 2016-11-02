using System;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal interface IRenderingScopeInterceptor
	{
		void Intercept(IRenderingScope renderingScope, IExpressionBuilder expressionBuilder, Action action);
	}
}