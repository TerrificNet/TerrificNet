using System.Collections.Generic;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class ScopedExpressionBuilder : IScopedExpressionBuilder
	{
		private readonly IExpressionBuilder _expressionBuilder;

		private readonly Stack<RenderingScope> _scopes = new Stack<RenderingScope>();
		private readonly IRenderingScopeInterceptor _scopeInterceptor;

		public ScopedExpressionBuilder(IExpressionBuilder expressionBuilder, IRenderingScopeInterceptor interceptor)
		{
			_expressionBuilder = expressionBuilder;
			_scopes.Push(new RenderingScope(null));
			_scopeInterceptor = interceptor;
		}

		private RenderingScope CurrentScope => _scopes.Peek();

		public void UseBinding(IBinding binding)
		{
			CurrentScope.UseBinding(binding);
		}

		public void Enter()
		{
			var bindingScope = CurrentScope.CreateChildScope();
			_scopes.Push(bindingScope);
		}

		public IRenderingScope Leave()
		{
			var currentScope = CurrentScope;

			_scopes.Pop();
			return currentScope;
		}

		public void Add(Expression expression)
		{
			CurrentScope.AddExpression(expression);
		}

		public void DefineVariable(ParameterExpression expression)
		{
			CurrentScope.DefineVariable(expression);
		}

		public Expression BuildExpression()
		{
			var renderingScope = CurrentScope;
			var scopeParameters = new ScopeParameters(_expressionBuilder, _scopeInterceptor);
			renderingScope.Process(scopeParameters);
			return _expressionBuilder.BuildExpression();
		}
	}
}