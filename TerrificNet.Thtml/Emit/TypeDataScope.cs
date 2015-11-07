using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TerrificNet.Thtml.Emit
{
	public class TypeDataScope : IDataBinder
	{
		private readonly Func<Expression, Expression> _expressionFactory;

		private TypeDataScope(Type dataContextType, Func<Expression, Expression> expressionFactory)
		{
			DataContextType = dataContextType;
			_expressionFactory = expressionFactory;
		}

		public static IDataBinder BinderFromType(Type type)
		{
			return new TypeDataScope(type, d => d);
		}

		public static IDataBinder BinderFromObject(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			Type type = obj.GetType();
			return BinderFromType(type);
		}

		public Expression BindStringToExpression(Expression dataContext)
		{
			return BindExpression<string>(dataContext);
		}

		private Expression BindExpression<T>(Expression dataContext)
		{
			var expression = _expressionFactory(dataContext);
			if (typeof(T) != expression.Type)
				throw new Exception($"Can not bind the {typeof(T).Name} to type {expression.Type}.");

			return expression;
		}

		public IEvaluator<bool> BindBoolean()
		{
			return CreateEvaluator<bool>(BindExpression<bool>);
		}

		public IEvaluator<IEnumerable> BindEnumerable(out IDataBinder childScope)
		{
			var param = Expression.Parameter(typeof(object));
			var context = Expression.Convert(param, DataContextType);
			var expression = _expressionFactory(context);

			childScope = Item(expression.Type);

			var lambda = Expression.Lambda<Func<object, IEnumerable>>(expression, param);
			return new EvaluatorFromLambda<IEnumerable>(lambda.Compile());
		}

		public Type DataContextType { get; }

		public IEvaluator<string> BindString()
		{
			return CreateEvaluator<string>(BindExpression<string>);
		}

		private IEvaluator<T> CreateEvaluator<T>(Func<Expression, Expression> binding)
		{
			var param = Expression.Parameter(typeof (object));
			var context = Expression.Convert(param, DataContextType);
			var expression = binding(context);
			var lambda = Expression.Lambda<Func<object, T>>(expression, param);
			return new EvaluatorFromLambda<T>(lambda.Compile());
		}

		public virtual IDataBinder Property(string propertyName)
		{
			return new TypeDataScope(DataContextType, d => Expression.Property(_expressionFactory(d), propertyName));
		}

		private static IDataBinder Item(Type resultType)
		{
			var enumerable = resultType.GetInterfaces().Union(new[] { resultType }).FirstOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			if (enumerable == null)
				return null;

			var type = enumerable.GetGenericArguments()[0];
			return new TypeDataScope(type, d => d);
		}

		private class EvaluatorFromLambda<T> : IEvaluator<T>
		{
			private readonly Func<object, T> _evaluationFunc;

			public EvaluatorFromLambda(Func<object, T> evaluationFunc)
			{
				_evaluationFunc = evaluationFunc;
			}

			public T Evaluate(object context)
			{
				return _evaluationFunc(context);
			}
		}
	}
}