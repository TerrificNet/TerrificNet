namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class ConditionalExpression : MustacheExpression
    {
        public MustacheExpression Expression { get; }

        public ConditionalExpression(MustacheExpression expression)
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