namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class IterationExpression : Expression
    {
        public Expression Expression { get; }

        public IterationExpression(Expression expression)
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