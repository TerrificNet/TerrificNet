using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public abstract class NodeVisitor : INodeVisitor
    {
        private readonly IExpressionVisitor _expressionVisitor;

        protected NodeVisitor(IExpressionVisitor expressionVisitor)
        {
            _expressionVisitor = expressionVisitor;
		}

		public virtual bool BeforeVisit(Document document)
		{
			return false;
		}

		public virtual void AfterVisit(Document document)
        {
        }

        public virtual bool BeforeVisit(Element element)
        {
            return false;
        }

        public virtual void AfterVisit(Element element)
        {
        }

        public virtual void Visit(ConstantAttributeContent attributeContent)
        {
        }

        public virtual bool BeforeVisit(Statement statement)
        {
            statement.Expression.Accept(_expressionVisitor);
            return false;
        }

        public virtual void AfterVisit(Statement statement)
        {
        }

        public virtual void Visit(TextNode textNode)
        {
        }

        public virtual void Visit(AttributeContentStatement constantAttributeContent)
        {
            constantAttributeContent.Expression.Accept(_expressionVisitor);
        }

        public virtual bool BeforeVisit(AttributeNode attributeNode)
        {
            return false;
        }

        public virtual void AfterVisit(AttributeNode attributeNode)
        {
        }

        public virtual bool BeforeVisit(CompositeAttributeContent compositeAttributeContent)
        {
            return false;
        }

        public virtual void AfterVisit(CompositeAttributeContent compositeAttributeContent)
        {
        }

        public virtual bool BeforeVisit(AttributeStatement attributeStatement)
        {
            attributeStatement.Expression.Accept(_expressionVisitor);
            return false;
        }

        public virtual void AfterVisit(AttributeStatement attributeStatement)
        {
        }
    }
}