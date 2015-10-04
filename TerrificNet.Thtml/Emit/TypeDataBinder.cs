using System;
using System.Linq.Expressions;
using MemberExpression = TerrificNet.Thtml.Parsing.Handlebars.MemberExpression;

namespace TerrificNet.Thtml.Emit
{
    public class TypeDataBinder : IDataBinder
    {
        private readonly Type _type;

        private TypeDataBinder(Type type)
        {
            _type = type;
        }

        public static IDataBinder BinderFromObject(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return new TypeDataBinder(obj.GetType());
        }

        public Func<IDataContext, string> Evaluate(MemberExpression memberExpression)
        {
            if (memberExpression.SubExpression == null)
            {
                var dataContextParameter = Expression.Parameter(typeof (IDataContext));
                var dataObj = Expression.TypeAs(Expression.Property(dataContextParameter, "Value"), _type);
                var memberAccess = Expression.Property(dataObj, memberExpression.Name);

                var lambda = Expression.Lambda<Func<IDataContext, string>>(memberAccess, dataContextParameter);
                return lambda.Compile();
            }

            return d => null;
        }
    }
}