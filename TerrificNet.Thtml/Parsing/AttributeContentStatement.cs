using System.Collections.Generic;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class AttributeContentStatement : AttributeContent
    {
        public MustacheExpression Expression { get; private set; }
        public IReadOnlyList<AttributeContent> Children { get; }

        public AttributeContentStatement(MustacheExpression expression, params AttributeContent[] children)
        {
            Expression = expression;
            Children = children ?? new AttributeContent[0];
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}