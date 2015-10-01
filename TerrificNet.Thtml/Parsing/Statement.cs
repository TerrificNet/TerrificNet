using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class Statement : Node
    {
        public Statement(Expression expression, params Node[] childNodes)
        {
            Expression = expression;
            ChildNodes = childNodes;
        }

        public Expression Expression { get; }

        public Node[] ChildNodes { get; }
    }
}