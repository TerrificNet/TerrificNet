using System;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class NullRenderingScopeInterceptor : IRenderingScopeInterceptor
	{
		public static readonly IRenderingScopeInterceptor Default = new NullRenderingScopeInterceptor();

		private NullRenderingScopeInterceptor()
		{
		}

		public void Intercept(IRenderingScope renderingScope, IExpressionBuilder expressionBuilder, Action action)
		{
			action();
		}
	}
}