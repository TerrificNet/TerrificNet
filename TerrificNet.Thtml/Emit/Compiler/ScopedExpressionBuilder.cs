using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class ScopedExpressionBuilder : IScopedExpressionBuilder
	{
		private readonly IExpressionBuilder _expressionBuilder;

		private readonly Stack<BindingScope> _scopes = new Stack<BindingScope>();

		public ScopedExpressionBuilder(IExpressionBuilder expressionBuilder)
		{
			_expressionBuilder = expressionBuilder;
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
			CurrentScope.BuildExpressions(_expressionBuilder);
			return _expressionBuilder.BuildExpression();
		}

		private class BindingScope : IBindingScope
		{
			private readonly List<IBinding> _bindings = new List<IBinding>();

			private readonly List<BindingScope> _childScopes = new List<BindingScope>();

			private readonly List<Action<IExpressionBuilder>> _buildActions = new List<Action<IExpressionBuilder>>();

			public BindingScope(IBindingScope parent)
			{
				Parent = parent;
			}

			public IEnumerable<IBinding> GetBindings()
			{
				return _bindings;
			}

			public IBindingScope Parent { get; }

			public IReadOnlyList<IBindingScope> Children => _childScopes;

			private void FlushExpressions(IExpressionBuilder expressionBuilder)
			{
				foreach (var innerEx in _buildActions)
					innerEx(expressionBuilder);
			}

			public void BuildExpressions(IExpressionBuilder expressionBuilder)
			{
				if (_buildActions.Count > 0 && _bindings.Count > 0)
				{
					var testExpression = BuildRenderingExpression();

					expressionBuilder.IfThen(testExpression, () => FlushExpressions(expressionBuilder));
				}
				else
				{
					FlushExpressions(expressionBuilder);
				}
			}

			public BindingScope CreateChildScope()
			{
				var bindingScope = new BindingScope(this);
				_childScopes.Add(bindingScope);
				_buildActions.Add(e => bindingScope.BuildExpressions(e));

				return bindingScope;
			}

			public void UseBinding(IBinding binding)
			{
				_bindings.Add(binding);
			}

			public void AddExpression(Expression expression)
			{
				_buildActions.Add(e => e.Add(expression));
			}

			public void DefineVariable(ParameterExpression expression)
			{
				_buildActions.Add(e => e.DefineVariable(expression));
			}

			private static ConstantExpression BuildRenderingExpression()
			{
				return Expression.Constant(true);
			}
		}
	}
}