namespace TerrificNet.Thtml.Parsing
{
    public class EvaluateBlockNode : Node
    {

        public EvaluateBlockNode(string expression, params Node[] childNodes)
        {
            Expression = expression;
            ChildNodes = childNodes;
        }

        public string Expression { get; }

        public Node[] ChildNodes { get; }
    }
}