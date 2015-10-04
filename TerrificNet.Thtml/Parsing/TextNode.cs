namespace TerrificNet.Thtml.Parsing
{
    public class TextNode : Node
    {
        public string Text { get; }

        public TextNode(string text)
        {
            Text = text;
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}