namespace TerrificNet.Thtml.Parsing
{
    public class AttributeNode
    {
        public string Name { get; }
        public string Value { get; }

        public AttributeNode(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}