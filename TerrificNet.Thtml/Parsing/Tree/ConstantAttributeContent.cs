namespace TerrificNet.Thtml.Parsing.Tree
{
    public class ConstantAttributeContent : AttributeContent
    {
        public string Text { get; private set; }

        public ConstantAttributeContent(string text)
        {
            Text = text;
        }
    }
}