using System;
using System.Linq;

namespace TerrificNet.Thtml.Emit
{
    public class DynamicDataBinder : DataBinderResult
    {
        private readonly IEvaluator _evalutor;

        public DynamicDataBinder()
        {
        }

        private DynamicDataBinder(IEvaluator evalutor)
        {
            _evalutor = evalutor;
        }

        public override bool TryCreateEvaluation<T>(out IEvaluater<T> evaluationFunc)
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
            return this;
        }

        private interface IEvaluator
        {
            object Evaluate(object obj);
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

                var type = value.GetType();
                var property = type.GetProperties().FirstOrDefault(p => p.Name.Equals(_propertyName, StringComparison.InvariantCultureIgnoreCase));
                if (property == null)
                    throw new Exception($"The type '{type}' doesn't contain a property with name '{_propertyName}'.");

                return property.GetValue(value);
            }
        }

        private class CastEvalutor<T> : IEvaluater<T>
        {
            private readonly IEvaluator _evalutor;

            public CastEvalutor(IEvaluator evalutor)
            {
                _evalutor = evalutor;
            }

            public T Evaluate(IDataContext context)
            {
                return (T) _evalutor.Evaluate(context.Value);
            }
        }
    }
}