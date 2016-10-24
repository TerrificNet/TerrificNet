using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public static class ExpressionBuilderExtensions
	{
		public static void Foreach(this IExpressionBuilder builder, Expression collection, Action<Expression> loopBuilder)
		{
			var elementType = typeof(object);
			var enumerableType = typeof(IEnumerable);
			var enumeratorType = typeof(IEnumerator);

			var enumeratorVar = builder.DefineVariable(enumeratorType);
			var getEnumeratorCall = Expression.Call(collection, enumerableType.GetTypeInfo().GetMethod(nameof(IEnumerable.GetEnumerator)));

			// The MoveNext method's actually on IEnumerator, not IEnumerator<T>
			var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetTypeInfo().GetMethod(nameof(IEnumerator.MoveNext)));

			var nextLabel = Expression.Label("Next");
			var breakLabel = Expression.Label("LoopBreak");

			builder.Add(Expression.Assign(enumeratorVar, Expression.Convert(getEnumeratorCall, enumeratorType)));

			builder.Add(Expression.Label(nextLabel));
			var condition = Expression.IfThen(Expression.Equal(moveNextCall, Expression.Constant(false)), Expression.Goto(breakLabel));
			builder.Add(condition);

			var loopVar = builder.DefineVariable(elementType);
			builder.Add(Expression.Assign(loopVar, Expression.Convert(Expression.Property(enumeratorVar, "Current"), elementType)));

			loopBuilder(loopVar);

			builder.Add(Expression.Goto(nextLabel));
			builder.Add(Expression.Label(breakLabel));
		}
	}
}
