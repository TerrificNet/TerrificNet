namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class ConditionalExpression : Expression
    {
        public Expression Expression { get; }

        public ConditionalExpression(Expression expression)
        {
            Expression = expression;
        }
    }
}