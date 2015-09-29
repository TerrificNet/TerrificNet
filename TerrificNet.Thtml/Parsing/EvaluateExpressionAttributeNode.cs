using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class EvaluateExpressionAttributeNode : ElementPart
    {
        public EvaluateExpression Expression { get; }
        public AttributeNode[] ChildNodes { get; }

        public EvaluateExpressionAttributeNode(EvaluateExpression expression, params AttributeNode[] childNodes)
        {
            Expression = expression;
            ChildNodes = childNodes;
        }
    }
}