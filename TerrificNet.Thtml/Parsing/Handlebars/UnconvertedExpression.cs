namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class UnconvertedExpression : Expression
    {
        public Expression Expression { get; set; }

        public UnconvertedExpression(Expression expression)
        {
            Expression = expression;
        }
    }
}