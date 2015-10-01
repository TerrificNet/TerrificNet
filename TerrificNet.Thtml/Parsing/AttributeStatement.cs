using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class AttributeStatement : ElementPart
    {
        public Expression Expression { get; }
        public AttributeNode[] ChildNodes { get; }

        public AttributeStatement(Expression expression, params AttributeNode[] childNodes)
        {
            Expression = expression;
            ChildNodes = childNodes;
        }
    }
}