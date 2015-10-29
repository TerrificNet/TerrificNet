using System;
using System.Collections;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
    public class DynamicDataScope : IDataScope
    {
        private readonly IEvaluator _evaluator;

        public DynamicDataScope()
        {
            _evaluator = new NullEvaluator();
        }

        private DynamicDataScope(IEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

	    public IEvaluator<string> BindString()
	    {
		    return new CastEvaluator<string>(_evaluator);
	    }

	    public IEvaluator<bool> BindBoolean()
	    {
			return new CastEvaluator<bool>(_evaluator);
		}

	    public IEvaluator<IEnumerable> BindEnumerable(out IDataScope childScope)
	    {
		    childScope = new DynamicDataScope();
		    return new CastEvaluator<IEnumerable>(_evaluator);
	    }

	    public virtual IDataScope Property(string propertyName, SyntaxNode node)
        {
            return new DynamicDataScope(new PropertyReflectionEvaluator(_evaluator, propertyName));
        }

        public virtual IDataScope Item()
        {
            return new DynamicDataScope();
        }

        private interface IEvaluator
        {
            object Evaluate(object obj);
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

            public T Evaluate(IDataContext context)
            {
                var result = _evaluator.Evaluate(context.Value);
                if (typeof (T) == typeof (string))
                {
                    if (result == null)
                        return (T) (object) null;

                    return (T) (object) result.ToString();
                }

                if (typeof (T) == typeof (bool))
                {
                    if (result is bool)
                        return (T) result;

                    return (T) (object) (result != null);
                }

                return (T) result;
            }
        }
    }
}