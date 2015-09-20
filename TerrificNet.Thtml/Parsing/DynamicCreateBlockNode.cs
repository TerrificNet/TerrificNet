namespace TerrificNet.Thtml.Parsing
{
    public class DynamicCreateBlockNode : CreateNode
    {

        public DynamicCreateBlockNode(string expression, params CreateNode[] childNodes)
        {
            Expression = expression;
            ChildNodes = childNodes;
        }

        public string Expression { get; }
        public CreateNode[] ChildNodes { get; }
    }
}