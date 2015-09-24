using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class EvaluateExpressionNode : Node
    {
        public EvaluateExpressionNode(EvaluateExpression expression)
        {
            Expression = expression;
        }

        public EvaluateExpression Expression { get; }
    }
}