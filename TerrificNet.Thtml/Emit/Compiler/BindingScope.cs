using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class BindingScope : IBindingScope
	{
		private readonly List<IBinding> _bindings = new List<IBinding>();

		private readonly List<BindingScope> _childScopes = new List<BindingScope>();

		private readonly List<Action<IExpressionBuilder, IBindingSupport>> _buildActions = new List<Action<IExpressionBuilder, IBindingSupport>>();

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

		private void FlushExpressions(IExpressionBuilder expressionBuilder, IBindingSupport bindingSupport)
		{
			foreach (var innerEx in _buildActions)
				innerEx(expressionBuilder, bindingSupport);
		}

		public void BuildExpressions(IExpressionBuilder expressionBuilder, IBindingSupport bindingSupport)
		{
			if (!IsSupported(bindingSupport))
				return;

			if (_buildActions.Count > 0 && _bindings.Count > 0)
			{
				var testExpression = Expression.Constant(true);

				expressionBuilder.IfThen(testExpression, () => FlushExpressions(expressionBuilder, bindingSupport));
			}
			else
			{
				FlushExpressions(expressionBuilder, bindingSupport);
			}
		}

		public BindingScope CreateChildScope()
		{
			var bindingScope = new BindingScope(this);
			_childScopes.Add(bindingScope);
			_buildActions.Add((e, s) => bindingScope.BuildExpressions(e, s));

			return bindingScope;
		}

		public void UseBinding(IBinding binding)
		{
			_bindings.Add(binding);
		}

		public void AddExpression(Expression expression)
		{
			_buildActions.Add((e, s) => e.Add(expression));
		}

		public void DefineVariable(ParameterExpression expression)
		{
			_buildActions.Add((e, s) => e.DefineVariable(expression));
		}

		public bool IsSupported(IBindingSupport builder)
		{
			return _bindings.All(builder.SupportsBinding);
		}
	}
}