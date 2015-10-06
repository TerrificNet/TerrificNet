namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class ConditionalExpression : MustacheExpression
    {
        public MustacheExpression Expression { get; }

        public ConditionalExpression(MustacheExpression expression)
        {
            Expression = expression;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            if (!visitor.BeforeVisit(this))
                return;

            Expression.Accept(visitor);

            visitor.AfterVisit(this);
        }
    }
}