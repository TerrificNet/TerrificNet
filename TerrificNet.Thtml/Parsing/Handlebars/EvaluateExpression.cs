using System.Linq.Expressions;

namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class EvaluateExpression
    {
        public AccessExpression Expression { get; }

        public EvaluateExpression(AccessExpression expression)
        {
            Expression = expression;
        }

    }
}