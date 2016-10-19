using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class AsyncExpressionBuilder
	{
		private readonly List<Expression> _expressions = new List<Expression>();

		public void Add(Expression expression)
		{
			if (expression.Type == typeof(void) || IsTask(expression))
				_expressions.Add(expression);
			else
				throw new ArgumentException("Only expressions with result void or Task are supported");
		}

		public Expression Complete()
		{
			var continueWith = new List<Expression>();
			for (int i = _expressions.Count - 1; i >= 0; i--)
			{
				var expression = _expressions[i];
				if (IsTask(expression))
				{
					expression = ContinueWith(expression, continueWith);
					continueWith.Clear();
				}
				continueWith.Insert(0, expression);
			}

			Expression result =  Expression.Block(continueWith);
			if (result.Type == typeof(void))
				result = ContinueWith(Expression.Constant(Task.CompletedTask), continueWith);

			return result;
		}

		private static Expression ContinueWith(Expression expression, IEnumerable<Expression> continueWith)
		{
			var block = Expression.Lambda<Action<Task>>(Expression.Block(continueWith), Expression.Parameter(typeof(Task)));
			expression = Expression.Call(expression, typeof(Task).GetTypeInfo().GetMethod("ContinueWith", new[] {typeof(Action<Task>)}), block);
			return expression;
		}

		private static bool IsTask(Expression expression)
		{
			return expression.Type == typeof(Task);
		}
	}
}