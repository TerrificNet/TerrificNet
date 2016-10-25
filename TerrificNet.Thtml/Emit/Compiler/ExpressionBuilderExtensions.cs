using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public static class ExpressionBuilderExtensions
	{
		public static void Foreach(this IExpressionBuilder builder, Expression collection, Action<Expression> loopBuilder, ParameterExpression loopVar)
		{
			var elementType = loopVar.Type;
			var enumerableType = typeof(IEnumerable);
			var enumeratorType = typeof(IEnumerator);

			var enumeratorExpression = Expression.Parameter(enumeratorType);
			builder.DefineVariable(enumeratorExpression);
			var getEnumeratorCall = Expression.Call(collection, enumerableType.GetTypeInfo().GetMethod(nameof(IEnumerable.GetEnumerator)));

			// The MoveNext method's actually on IEnumerator, not IEnumerator<T>
			var moveNextCall = Expression.Call(enumeratorExpression, typeof(IEnumerator).GetTypeInfo().GetMethod(nameof(IEnumerator.MoveNext)));

			var nextLabel = Expression.Label("Next");
			var breakLabel = Expression.Label("LoopBreak");

			builder.Add(Expression.Assign(enumeratorExpression, Expression.Convert(getEnumeratorCall, enumeratorType)));

			builder.Add(Expression.Label(nextLabel));
			var condition = Expression.IfThen(Expression.Equal(moveNextCall, Expression.Constant(false)), Expression.Goto(breakLabel));
			builder.Add(condition);

			builder.DefineVariable(loopVar);
			builder.Add(Expression.Assign(loopVar, Expression.Convert(Expression.Property(enumeratorExpression, "Current"), elementType)));

			loopBuilder(loopVar);

			builder.Add(Expression.Goto(nextLabel));
			builder.Add(Expression.Label(breakLabel));
		}

		public static void IfThen(this IExpressionBuilder builder, Expression testExpression, Action body)
		{
			var breakLabel = Expression.Label("End");

			var condition = Expression.IfThen(Expression.Not(testExpression), Expression.Goto(breakLabel));
			builder.Add(condition);

			body();

			builder.Add(Expression.Label(breakLabel));
		}
	}
}
