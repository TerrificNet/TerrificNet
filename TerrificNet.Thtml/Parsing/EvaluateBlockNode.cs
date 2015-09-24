using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class EvaluateBlockNode : Node
    {

        public EvaluateBlockNode(EvaluateExpression expression, params Node[] childNodes)
        {
            Expression = expression;
            ChildNodes = childNodes;
        }

        public EvaluateExpression Expression { get; }

        public Node[] ChildNodes { get; }
    }
}