namespace TerrificNet.Thtml.Parsing.Tree
{
    public class CompositeAttributeContent : AttributeContent
    {
        public AttributeContent[] ContentParts { get; }

        public CompositeAttributeContent(params AttributeContent[] contentParts)
        {
            ContentParts = contentParts;
        }
    }
}