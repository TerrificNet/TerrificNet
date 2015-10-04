namespace TerrificNet.Thtml.Parsing
{
    public class ConstantAttributeContent : AttributeContent
    {
        public string Text { get; private set; }

        public ConstantAttributeContent(string text)
        {
            Text = text;
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}