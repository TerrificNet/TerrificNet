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
		private readonly ParameterExpression _dataContextParameter;
		private readonly Expression _memberAccess;

		private TypeDataScope(Type type)
		{
			_dataContextParameter = Expression.Parameter(typeof(object));
			_memberAccess = Expression.ConvertChecked(_dataContextParameter, type);

			ResultType = type;
		}

		private TypeDataScope(Expression expression, ParameterExpression parameter) : this(expression.Type)
		{
			_dataContextParameter = parameter;
			_memberAccess = expression;
		}

		public Type ResultType { get; }

		public static IDataBinder BinderFromType(Type type)
		{
			return new TypeDataScope(type);
		}

		public static IDataBinder BinderFromObject(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			return new TypeDataScope(obj.GetType());
		}

		private Func<object, T> CreateEvaluation<T>()
		{
			var lambda = Expression.Lambda<Func<object, T>>(_memberAccess, _dataContextParameter);
			return lambda.Compile();
		}

		private IEvaluator<T> Bind<T>()
		{
			if (typeof(T) != ResultType)
				throw new Exception($"Can not bind the ${typeof(T).Name} to type ${ResultType}.");

			return new EvaluatorFromLambda<T>(CreateEvaluation<T>());
		}

		public IEvaluator<bool> BindBoolean()
		{
			return Bind<bool>();
		}

		public IEvaluator<IEnumerable> BindEnumerable(out IDataBinder childScope)
		{
			childScope = Item();
			return BindEnumerable();
		}

		public IEvaluator<string> BindString()
		{
			return Bind<string>();
		}

		public IEvaluator<IEnumerable> BindEnumerable()
		{
			if (!typeof(IEnumerable).IsAssignableFrom(ResultType) || typeof(string).IsAssignableFrom(ResultType))
				throw new Exception($"Can not bind the enumerable to type ${ResultType}.");

			return new EvaluatorFromLambda<IEnumerable>(CreateEvaluation<IEnumerable>());
		}

		public virtual IDataBinder Property(string propertyName)
		{
			return new TypeDataScope(Expression.Property(_memberAccess, propertyName), _dataContextParameter);
		}

		private IDataBinder Item()
		{
			var enumerable = ResultType.GetInterfaces().Union(new[] { ResultType }).FirstOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			if (enumerable == null)
				return null;

			return new TypeDataScope(enumerable.GetGenericArguments()[0]);
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