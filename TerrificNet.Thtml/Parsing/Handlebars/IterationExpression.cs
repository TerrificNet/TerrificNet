namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class IterationExpression : MustacheExpression
    {
        public MustacheExpression Expression { get; }

        public IterationExpression(MustacheExpression expression)
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