using System.Collections.Generic;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class ScopedExpressionBuilder : IScopedExpressionBuilder
	{
		private readonly IExpressionBuilder _expressionBuilder;
		private readonly IBindingSupport _bindingSupport;

		private readonly Stack<BindingScope> _scopes = new Stack<BindingScope>();

		public ScopedExpressionBuilder(IExpressionBuilder expressionBuilder, IBindingSupport bindingSupport)
		{
			_expressionBuilder = expressionBuilder;
			_bindingSupport = bindingSupport;
			_scopes.Push(new BindingScope(null));
		}

		private BindingScope CurrentScope => _scopes.Peek();

		public void UseBinding(IBinding binding)
		{
			CurrentScope.UseBinding(binding);
		}

		public void Enter()
		{
			var bindingScope = CurrentScope.CreateChildScope();
			_scopes.Push(bindingScope);
		}

		public IBindingScope Leave()
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
			CurrentScope.BuildExpressions(_expressionBuilder, _bindingSupport);
			return _expressionBuilder.BuildExpression();
		}
	}
}