namespace TerrificNet.Thtml.Parsing
{
    public class EvaluateExpressionNode : Node
    {
        public EvaluateExpressionNode(string expression)
        {
            Expression = expression;
        }

        public string Expression { get; }
    }
}