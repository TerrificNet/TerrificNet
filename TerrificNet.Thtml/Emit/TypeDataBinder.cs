using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit
{
    public class TypeDataBinder : DataBinderResult
    {
        private readonly ParameterExpression _dataContextParameter;
        private readonly Expression _memberAccess;

        private TypeDataBinder(Type type) : base(type)
        {
            _dataContextParameter = Expression.Parameter(typeof(IDataContext));
            _memberAccess = Expression.ConvertChecked(Expression.Property(_dataContextParameter, "Value"), type);
        }

        private TypeDataBinder(Expression expression, ParameterExpression parameter) : base(expression.Type)
        {
            _dataContextParameter = parameter;
            _memberAccess = expression;
        }

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

        public override Func<IDataContext, T> CreateEvaluation<T>()
        {
            var lambda = Expression.Lambda<Func<IDataContext, T>>(_memberAccess, _dataContextParameter);
            return lambda.Compile();
        }

        public override DataBinderResult Evaluate(string propertyName)
        {
            return new TypeDataBinder(Expression.Property(_memberAccess, propertyName), _dataContextParameter);
        }

        public override DataBinderResult Item()
        {
            var enumerable = this.ResultType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IEnumerable<>));

            return new TypeDataBinder(enumerable.GetGenericArguments()[0]);
        }
    }
}