using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Binding
{
	public class DynamicDataBinder : IDataBinder
	{
		private readonly IEvaluator _evaluator;
		private static readonly MethodInfo EvaluateMethodString;
		private static readonly MethodInfo EvaluateMethodBool;
		private static readonly MethodInfo EvaluateMethodEnumerable;

		public DynamicDataBinder()
		{
			_evaluator = new NullEvaluator();
		}

		private DynamicDataBinder(IEvaluator evaluator)
		{
			_evaluator = evaluator;
		}

		static DynamicDataBinder()
		{
			EvaluateMethodString = ExpressionHelper.GetMethodInfo<IEvaluator<string>>(i => i.Evaluate(null));
			EvaluateMethodBool = ExpressionHelper.GetMethodInfo<IEvaluator<bool>>(i => i.Evaluate(null));
			EvaluateMethodEnumerable = ExpressionHelper.GetMethodInfo<IEvaluator<IEnumerable>>(i => i.Evaluate(null));
		}

		private IEvaluator<string> BindString()
		{
			return new CastEvaluator<string>(_evaluator);
		}

		public Expression BindString(Expression dataContext)
		{
			return Expression.Call(Expression.Constant(BindString()), EvaluateMethodString, dataContext);
		}

		public Expression BindBoolean(Expression dataContext)
		{
			return Expression.Call(Expression.Constant(BindBoolean()), EvaluateMethodBool, dataContext);
		}

		private IEvaluator<bool> BindBoolean()
		{
			return new CastEvaluator<bool>(_evaluator);
		}

		public IDataBinder Item()
		{
			IDataBinder childScope = new DynamicDataBinder();
			return childScope;
		}

		private IEvaluator<IEnumerable> BindEnumerable2(out IDataBinder childScope)
		{
			childScope = new DynamicDataBinder();
			return new CastEvaluator<IEnumerable>(_evaluator);
		}

		public Expression BindEnumerable(Expression dataContext)
		{
			IDataBinder childScope;
			return Expression.Call(Expression.Constant(BindEnumerable2(out childScope)), EvaluateMethodEnumerable, dataContext);
		}

		public Type ResultType => typeof(object);

		public virtual IDataBinder Property(string propertyName)
		{
			return new DynamicDataBinder(new PropertyReflectionEvaluator(_evaluator, propertyName));
		}

		private interface IEvaluator
		{
			object Evaluate(object obj);
		}

		private interface IEvaluator<out T>
		{
			T Evaluate(object context);
		}

		private class NullEvaluator : IEvaluator
		{
			public object Evaluate(object obj)
			{
				return obj;
			}
		}

		private class PropertyReflectionEvaluator : IEvaluator
		{
			private readonly IEvaluator _objectEvaluator;
			private readonly string _propertyName;

			public PropertyReflectionEvaluator(IEvaluator objectEvaluator, string propertyName)
			{
				_objectEvaluator = objectEvaluator;
				_propertyName = propertyName;
			}

			public object Evaluate(object value)
			{
				if (_objectEvaluator != null)
					value = _objectEvaluator.Evaluate(value);

				if (value == null)
					throw new Exception($"Unable to bind property '{_propertyName}' null.");

				var binder = TypeHelperBinder.GetBinder(value, _propertyName);
				if (binder == null)
					throw new Exception($"The type '{value.GetType()}' doesn't contain a property with name '{_propertyName}'.");

				return binder(value);
			}
		}

		private class CastEvaluator<T> : IEvaluator<T>
		{
			private readonly IEvaluator _evaluator;

			public CastEvaluator(IEvaluator evaluator)
			{
				_evaluator = evaluator;
			}

			public T Evaluate(object context)
			{
				var result = _evaluator.Evaluate(context);
				if (typeof(T) == typeof(string))
				{
					if (result == null)
						return (T)(object)null;

					return (T)(object)result.ToString();
				}

				if (typeof(T) == typeof(bool))
				{
					if (result is bool)
						return (T)result;

					return (T)(object)(result != null);
				}

				return (T)result;
			}
		}
	}
}