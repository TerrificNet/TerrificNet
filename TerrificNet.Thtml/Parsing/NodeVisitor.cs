using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public abstract class NodeVisitor : INodeVisitor
    {
        public virtual void Visit(Element element)
        {
        }

        public virtual void Visit(ConstantAttributeContent attributeContent)
        {
        }

        public virtual void Visit(CallHelperExpression callHelperExpression)
        {
        }

        public virtual void Visit(ConditionalExpression conditionalExpression)
        {
        }

        public virtual void Visit(IterationExpression iterationExpression)
        {
        }

        public virtual void Visit(MemberExpression memberExpression)
        {
        }

        public virtual void Visit(UnconvertedExpression unconvertedExpression)
        {
        }

        public virtual void Visit(Statement statement)
        {
        }

        public virtual void Visit(TextNode textNode)
        {
        }

        public virtual void Visit(AttributeContentStatement constantAttributeContent)
        {
        }

        public virtual void Visit(AttributeNode attributeNode)
        {
        }

        public virtual void Visit(CompositeAttributeContent compositeAttributeContent)
        {
        }

        public virtual void Visit(AttributeStatement attributeStatement)
        {
        }

        public virtual void Visit(Document document)
        {
        }
    }
}