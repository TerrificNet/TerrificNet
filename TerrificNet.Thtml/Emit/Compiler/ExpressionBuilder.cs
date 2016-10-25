using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class ExpressionBuilder : IExpressionBuilder
	{
		private readonly List<Expression> _expressions = new List<Expression>();
		private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();

		public void Add(Expression expression)
		{
			if (expression == null)
				throw new ArgumentNullException(nameof(expression));

			if (expression.Type != typeof(void) && expression.NodeType != ExpressionType.Assign)
				throw new ArgumentException("Only void expressions are supported.", nameof(expression));

			_expressions.Add(expression);
		}

		public void DefineVariable(ParameterExpression expression)
		{
			_variables.Add(expression);
		}

		public Expression BuildExpression()
		{
			return Expression.Block(_variables, _expressions);
		}
	}
}
