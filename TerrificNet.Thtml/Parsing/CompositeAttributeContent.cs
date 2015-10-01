namespace TerrificNet.Thtml.Parsing
{
    public class CompositeAttributeContent : AttributeContent
    {
        public AttributeContent[] ContentParts { get; private set; }

        public CompositeAttributeContent(params AttributeContent[] contentParts)
        {
            ContentParts = contentParts;
        }
    }
}