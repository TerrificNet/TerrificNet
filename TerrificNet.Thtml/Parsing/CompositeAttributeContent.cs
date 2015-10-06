namespace TerrificNet.Thtml.Parsing
{
    public class CompositeAttributeContent : AttributeContent
    {
        public AttributeContent[] ContentParts { get; private set; }

        public CompositeAttributeContent(params AttributeContent[] contentParts)
        {
            ContentParts = contentParts;
        }

        public override void Accept(INodeVisitor visitor)
        {
            if (!visitor.BeforeVisit(this))
                return;

            foreach (var part in ContentParts)
                part.Accept(visitor);

            visitor.AfterVisit(this);
        }
    }
}