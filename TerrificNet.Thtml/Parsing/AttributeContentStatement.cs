using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class AttributeContentStatement : AttributeContent
    {
        public Expression Expression { get; private set; }

        public AttributeContentStatement(Expression expression)
        {
            Expression = expression;
        }

        public override void Accept(INodeVisitor visitor)
        {
            this.Expression.Accept(visitor);
            visitor.Visit(this);
        }
    }
}