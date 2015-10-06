using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class AttributeContentStatement : AttributeContent
    {
        public MustacheExpression Expression { get; private set; }

        public AttributeContentStatement(MustacheExpression expression)
        {
            Expression = expression;
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}