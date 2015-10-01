using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class Statement : Node
    {
        public Statement(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; }
    }
}