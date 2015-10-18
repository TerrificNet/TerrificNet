using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TerrificNet.Thtml.Emit
{
    public class TypeDataBinder : DataBinderResult
    {
        private readonly ParameterExpression _dataContextParameter;
        private readonly Expression _memberAccess;

        private TypeDataBinder(Type type)
        {
            _dataContextParameter = Expression.Parameter(typeof(IDataContext));
            _memberAccess = Expression.ConvertChecked(Expression.Property(_dataContextParameter, "Value"), type);

            ResultType = type;
        }

        private TypeDataBinder(Expression expression, ParameterExpression parameter) : this(expression.Type)
        {
            _dataContextParameter = parameter;
            _memberAccess = expression;
        }

        internal Type ResultType { get; }

        public static IDataBinder BinderFromType(Type type)
        {
            return new TypeDataBinder(type);
        }

        public static IDataBinder BinderFromObject(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return new TypeDataBinder(obj.GetType());
        }

        private Func<IDataContext, T> CreateEvaluation<T>()
        {
            var lambda = Expression.Lambda<Func<IDataContext, T>>(_memberAccess, _dataContextParameter);
            return lambda.Compile();
        }

        public override bool TryCreateEvaluation<T>(out IEvaluator<T> evaluationFunc)
        {
            evaluationFunc = null;

            if (typeof (T) == typeof (IEnumerable))
            {
                if (!ResultType.GetInterfaces().Contains(typeof (IEnumerable)))
                    return false;

                evaluationFunc = new EvaluatorFromLambda<T>(CreateEvaluation<T>());
                return true;
            }

            if (typeof (T) == ResultType)
            {
                evaluationFunc = new EvaluatorFromLambda<T>(CreateEvaluation<T>());
                return true;
            }

            return false;
        }

        public override DataBinderResult Property(string propertyName)
        {
            return new TypeDataBinder(Expression.Property(_memberAccess, propertyName), _dataContextParameter);
        }

        public override DataBinderResult Item()
        {
            var enumerable = ResultType.GetInterfaces().Union(new [] { ResultType }).FirstOrDefault(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof (IEnumerable<>));
            if (enumerable == null)
                return null;

            return new TypeDataBinder(enumerable.GetGenericArguments()[0]);
        }

        private class EvaluatorFromLambda<T> : IEvaluator<T>
        {
            private readonly Func<IDataContext, T> _evalutionFunc;

            public EvaluatorFromLambda(Func<IDataContext, T> evalutionFunc)
            {
                _evalutionFunc = evalutionFunc;
            }

            public T Evaluate(IDataContext context)
            {
                return _evalutionFunc(context);
            }
        }
    }
}