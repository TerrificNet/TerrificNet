namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class Unconverted : Expression
    {
        public Expression Expression { get; set; }

        public Unconverted(Expression expression)
        {
            Expression = expression;
        }
    }
}