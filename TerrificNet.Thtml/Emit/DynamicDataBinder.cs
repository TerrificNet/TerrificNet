using System;
using TerrificNet.Thtml.Binding;

namespace TerrificNet.Thtml.Emit
{
    public class DynamicDataBinder : DataBinderResult
    {
        private readonly IEvaluator _evalutor;

        public DynamicDataBinder()
        {
            _evalutor = new NullEvaluater();
        }

        private DynamicDataBinder(IEvaluator evalutor)
        {
            _evalutor = evalutor;
        }

        public override bool TryCreateEvaluation<T>(out IEvaluator<T> evaluationFunc)
        {
            evaluationFunc = new CastEvalutor<T>(_evalutor);
            return true;
        }

        public override DataBinderResult Property(string propertyName)
        {
            return new DynamicDataBinder(new PropertyReflectionEvalutor(_evalutor, propertyName));
        }

        public override DataBinderResult Item()
        {
            return new DynamicDataBinder();
        }

        private interface IEvaluator
        {
            object Evaluate(object obj);
        }

        private class NullEvaluater : IEvaluator
        {
            public object Evaluate(object obj)
            {
                return obj;
            }
        }

        private class PropertyReflectionEvalutor : IEvaluator
        {
            private readonly IEvaluator _objectEvalutor;
            private readonly string _propertyName;

            public PropertyReflectionEvalutor(IEvaluator objectEvalutor, string propertyName)
            {
                _objectEvalutor = objectEvalutor;
                _propertyName = propertyName;
            }

            public object Evaluate(object value)
            {
                if (_objectEvalutor != null)
                    value = _objectEvalutor.Evaluate(value);

                if (value == null)
                    throw new Exception($"Unable to bind property '{_propertyName}' null.");

                var binder = TypeHelperBinder.GetBinder(value, _propertyName);
                if (binder == null)
                    throw new Exception($"The type '{value.GetType()}' doesn't contain a property with name '{_propertyName}'.");

                return binder(value);
            }
        }

        private class CastEvalutor<T> : IEvaluator<T>
        {
            private readonly IEvaluator _evalutor;

            public CastEvalutor(IEvaluator evalutor)
            {
                _evalutor = evalutor;
            }

            public T Evaluate(IDataContext context)
            {
                var result = _evalutor.Evaluate(context.Value);
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