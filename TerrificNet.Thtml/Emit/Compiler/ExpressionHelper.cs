using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public static class ExpressionHelper
	{
		private static readonly MethodInfo WriteMethodInfo = GetMethodInfo<TextWriter>(i => i.Write(""));

		public static Expression Write(Expression writer, Expression inputExpression)
		{
			return Expression.Call(writer, WriteMethodInfo, inputExpression);
		}

		public static Expression Write(Expression writer, string value)
		{
			var param = Expression.Constant(value);
			return Expression.Call(writer, WriteMethodInfo, param);
		}

		public static MethodInfo GetMethodInfo<T, TOut>(Expression<Func<T, TOut>> expression)
		{
			return GetMethodInfoInternal(expression);
		}

		public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
		{
			return GetMethodInfoInternal(expression);
		}

		private static MethodInfo GetMethodInfoInternal(LambdaExpression expression)
		{
			var member = expression.Body as MethodCallExpression;

			if (member != null)
				return member.Method;

			throw new ArgumentException("Expression is not a method", nameof(expression));
		}
	}
}