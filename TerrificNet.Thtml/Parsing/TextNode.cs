namespace TerrificNet.Thtml.Parsing
{
    public class TextNode : Node
    {
        public string Text { get; }

        public TextNode(string text)
        {
            Text = text;
        }
    }
}