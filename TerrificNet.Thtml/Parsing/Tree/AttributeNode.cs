namespace TerrificNet.Thtml.Parsing.Tree
{
    public class AttributeNode : ElementPart
    {
        public string Name { get; }
        public AttributeContent Value { get; }

        public AttributeNode(string name, AttributeContent value)
        {
            Name = name;
            Value = value;
        }

        public AttributeNode(string name, string value) : this(name, new ConstantAttributeContent(value))
        {
        }
    }
}