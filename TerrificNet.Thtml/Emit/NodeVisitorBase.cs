using System;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Emit
{
    public class NodeVisitorBase<T> : INodeVisitor<T>
    {
        public virtual T Visit(Element element)
        {
	        throw new NotSupportedException();
        }

        public virtual T Visit(TextNode textNode)
        {
			throw new NotSupportedException();
		}

        public virtual T Visit(Statement statement)
        {
			throw new NotSupportedException();
		}

        public virtual T Visit(AttributeNode attributeNode)
        {
			throw new NotSupportedException();
		}

        public virtual T Visit(AttributeContentStatement constantAttributeContent)
        {
			throw new NotSupportedException();
		}

        public virtual T Visit(ConstantAttributeContent attributeContent)
        {
			throw new NotSupportedException();
		}

        public virtual T Visit(Document document)
        {
			throw new NotSupportedException();
		}

        public virtual T Visit(CompositeAttributeContent compositeAttributeContent)
        {
			throw new NotSupportedException();
		}

        public virtual T Visit(CallHelperExpression callHelperExpression)
        {
			throw new NotSupportedException();
		}

        public virtual T Visit(UnconvertedExpression unconvertedExpression)
        {
			throw new NotSupportedException();
		}

        public virtual T Visit(AttributeStatement attributeStatement)
        {
			throw new NotSupportedException();
		}

        public virtual T Visit(IterationExpression iterationExpression)
        {
			throw new NotSupportedException();
		}

        public virtual T Visit(ConditionalExpression conditionalExpression)
        {
			throw new NotSupportedException();
		}

        public virtual T Visit(MemberExpression memberExpression)
        {
			throw new NotSupportedException();
		}
    }
}