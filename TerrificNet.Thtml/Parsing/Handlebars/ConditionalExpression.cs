namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class ConditionalExpression : Expression
    {
        public Expression Expression { get; }

        public ConditionalExpression(Expression expression)
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