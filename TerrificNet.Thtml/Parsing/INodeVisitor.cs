namespace TerrificNet.Thtml.Parsing
{
    public interface INodeVisitor
    {
        bool BeforeVisit(Document document);
        void AfterVisit(Document document);
        bool BeforeVisit(Element element);
        void AfterVisit(Element element);
        void Visit(ConstantAttributeContent attributeContent);
        bool BeforeVisit(Statement statement);
        void AfterVisit(Statement statement);
        void Visit(TextNode textNode);
        void Visit(AttributeContentStatement constantAttributeContent);
        bool BeforeVisit(AttributeNode attributeNode);
        void AfterVisit(AttributeNode attributeNode);
        bool BeforeVisit(CompositeAttributeContent compositeAttributeContent);
        void AfterVisit(CompositeAttributeContent compositeAttributeContent);
        bool BeforeVisit(AttributeStatement attributeStatement);
        void AfterVisit(AttributeStatement attributeStatement);
    }
}