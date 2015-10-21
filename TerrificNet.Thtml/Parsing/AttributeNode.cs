namespace TerrificNet.Thtml.Parsing
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

	    public override void Accept(INodeVisitor visitor)
	    {
		    visitor.Visit(this);
	    }
    }
}