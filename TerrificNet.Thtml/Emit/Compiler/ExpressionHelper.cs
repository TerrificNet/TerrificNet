using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public static class ExpressionHelper
	{
		private static readonly MethodInfo WriteMethodInfo = GetMethodInfo<TextWriter>(i => i.Write(""));

		public static Expression ForEach(Expression collection, ParameterExpression loopVar, IEnumerable<Expression> loopContent)
		{
			var elementType = loopVar.Type;
			var enumerableType = typeof(IEnumerable);
			var enumeratorType = typeof(IEnumerator);

			var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
			var getEnumeratorCall = Expression.Call(collection, enumerableType.GetTypeInfo().GetMethod("GetEnumerator"));
			var enumeratorAssign = Expression.Assign(enumeratorVar, Expression.Convert(getEnumeratorCall, enumeratorType));

			Expression listAssign = Expression.Empty();
			Expression loopExpression = Expression.Block(loopContent);

			// The MoveNext method's actually on IEnumerator, not IEnumerator<T>
			var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetTypeInfo().GetMethod("MoveNext"));

			var breakLabel = Expression.Label("LoopBreak");

			var loop = Expression.Block(new[] {enumeratorVar},
				enumeratorAssign,
				listAssign,
				Expression.Loop(
					Expression.IfThenElse(
						Expression.Equal(moveNextCall, Expression.Constant(true)),
						Expression.Block(new[] {loopVar},
							Expression.Assign(loopVar, Expression.Convert(Expression.Property(enumeratorVar, "Current"), elementType)),
							loopExpression
						),
						Expression.Break(breakLabel)
					),
					breakLabel),
				Expression.Empty()
			);

			return loop;
		}

		public static Expression Write(Expression writer, Expression inputExpression)
		{
			return Expression.Call(writer, WriteMethodInfo, inputExpression);
		}

		public static Expression Write(Expression writer, string value)
		{
			var param = Expression.Constant(value);
			return Expression.Call(writer, WriteMethodInfo, param);
		}

		public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
		{
			var member = expression.Body as MethodCallExpression;

			if (member != null)
				return member.Method;

			throw new ArgumentException("Expression is not a method", nameof(expression));
		}
	}
}