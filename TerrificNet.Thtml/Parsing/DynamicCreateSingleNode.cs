namespace TerrificNet.Thtml.Parsing
{
    public class DynamicCreateSingleNode : CreateNode
    {
        public DynamicCreateSingleNode(string expression)
        {
            Expression = expression;
        }

        public string Expression { get; }
    }
}