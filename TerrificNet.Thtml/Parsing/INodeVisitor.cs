using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public interface INodeVisitor
    {
        void Visit(Document document);
        void Visit(Element element);
        void Visit(ConstantAttributeContent attributeContent);
        void Visit(CallHelperExpression callHelperExpression);
        void Visit(ConditionalExpression conditionalExpression);
        void Visit(IterationExpression iterationExpression);
        void Visit(MemberExpression memberExpression);
        void Visit(UnconvertedExpression unconvertedExpression);
        void Visit(Statement statement);
        void Visit(TextNode textNode);
        void Visit(AttributeContentStatement constantAttributeContent);
        void Visit(AttributeNode attributeNode);
        void Visit(CompositeAttributeContent compositeAttributeContent);
        void Visit(AttributeStatement attributeStatement);
    }
}