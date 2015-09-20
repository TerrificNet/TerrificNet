namespace TerrificNet.Thtml.Parsing
{
    public class DynamicCreateNode : CreateNode
    {
        public DynamicCreateNode(string expression)
        {
            Expression = expression;
        }

        public string Expression { get; }
    }
}