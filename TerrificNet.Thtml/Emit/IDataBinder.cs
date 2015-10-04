using System;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Emit
{
    public interface IDataBinder
    {
        Func<IDataContext, string> Evaluate(MemberExpression memberExpression);
    }
}