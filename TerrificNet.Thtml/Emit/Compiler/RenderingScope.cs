using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class RenderingScope : IRenderingScope
	{
		private readonly List<IBinding> _bindings = new List<IBinding>();

		private readonly List<RenderingScope> _childScopes = new List<RenderingScope>();

		private readonly List<Action<ScopeParameters>> _buildActions = new List<Action<ScopeParameters>>();

		public RenderingScope(IRenderingScope parent, IBinding id)
		{
			Parent = parent;
			Id = id;
		}

		public IEnumerable<IBinding> GetBindings()
		{
			return _bindings;
		}

		public IRenderingScope Parent { get; }

		public IBinding Id { get; }

		public IReadOnlyList<IRenderingScope> Children => _childScopes;

		public RenderingScope CreateChildScope(IBinding idBinding)
		{
			var bindingScope = new RenderingScope(this, idBinding);
			_childScopes.Add(bindingScope);
			_buildActions.Add(p => bindingScope.Process(p));

			return bindingScope;
		}

		public void UseBinding(IBinding binding)
		{
			_bindings.Add(binding);
		}

		public void AddExpression(Expression expression)
		{
			_buildActions.Add(p => p.ExpressionBuilder.Add(expression));
		}

		public void DefineVariable(ParameterExpression expression)
		{
			_buildActions.Add(e => e.ExpressionBuilder.DefineVariable(expression));
		}

		public bool IsEmpty()
		{
			return _buildActions.Count == 0;
		}

		public void Process(ScopeParameters scopeParameters)
		{
			var scopeInterceptor = scopeParameters.Interceptor;
			scopeInterceptor.Intercept(this, scopeParameters.ExpressionBuilder, () => ProcessAction(scopeParameters));
		}

		private void ProcessAction(ScopeParameters parameters)
		{
			foreach (var innerEx in _buildActions)
				innerEx(parameters);
		}
	}
}