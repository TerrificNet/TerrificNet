using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TerrificNet.Thtml.Emit
{
	public class TypeDataBinder : IDataBinder
	{
		private readonly Func<Expression, Expression> _expressionFactory;

		private TypeDataBinder(Type dataContextType, Func<Expression, Expression> expressionFactory)
		{
			ResultType = dataContextType;
			_expressionFactory = expressionFactory;
		}

		public static IDataBinder BinderFromType(Type type)
		{
			return new TypeDataBinder(type, d => d);
		}

		public static IDataBinder BinderFromObject(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			Type type = obj.GetType();
			return BinderFromType(type);
		}

		public Expression BindString(Expression dataContext)
		{
			return BindExpression<string>(dataContext);
		}

		public Expression BindBoolean(Expression dataContext)
		{
			return BindExpression<bool>(dataContext);
		}

		private Expression BindExpression<T>(Expression dataContext)
		{
			var expression = _expressionFactory(dataContext);
			if (typeof(T) != expression.Type)
				throw new Exception($"Can not bind the {typeof(T).Name} to type {expression.Type}.");

			return expression;
		}

		public IDataBinder Item()
		{
			var param = Expression.Parameter(typeof(object));
			var context = Expression.Convert(param, ResultType);
			var expression = _expressionFactory(context);

			var childScope = Item(expression.Type);
			return childScope;
		}

		public Expression BindEnumerable(Expression dataContext)
		{
			return _expressionFactory(dataContext);
		}

		public Type ResultType { get; }

		public virtual IDataBinder Property(string propertyName)
		{
			return new TypeDataBinder(ResultType, d => Expression.Property(_expressionFactory(d), propertyName));
		}

		private static IDataBinder Item(Type resultType)
		{
			var enumerable = resultType.GetTypeInfo().GetInterfaces().Union(new[] {resultType}).FirstOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			if (enumerable == null)
				return null;

			var type = enumerable.GetTypeInfo().GetGenericArguments()[0];
			return new TypeDataBinder(type, d => d);
		}
	}
}