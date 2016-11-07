using System;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class EmptyScopedExpressionBuilder : IScopedExpressionBuilder
	{
		private readonly IExpressionBuilder _expressionBuilder;
		private readonly IBindingSupport _bindingSupport;

		public EmptyScopedExpressionBuilder(IExpressionBuilder expressionBuilder, IBindingSupport bindingSupport)
		{
			_expressionBuilder = expressionBuilder;
			_bindingSupport = bindingSupport;
		}

		public void Add(Expression expression)
		{
			_expressionBuilder.Add(expression);
		}

		public void DefineVariable(ParameterExpression expression)
		{
			_expressionBuilder.DefineVariable(expression);
		}

		public Expression BuildExpression()
		{
			return _expressionBuilder.BuildExpression();
		}

		public void UseBinding(IBinding binding)
		{
			if (!_bindingSupport.SupportsBinding(binding))
				throw new NotSupportedException($"The given binding with path '{binding.Path}' isn't supported.");
		}

		public void Enter()
		{
		}

		public void Enter(IBinding id)
		{
		}

		public IRenderingScope Leave()
		{
			return null;
		}
	}
}