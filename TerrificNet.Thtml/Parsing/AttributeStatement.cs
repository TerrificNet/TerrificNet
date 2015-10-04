using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class AttributeStatement : ElementPart
    {
        public MustacheExpression Expression { get; }
        public AttributeNode[] ChildNodes { get; }

        public AttributeStatement(MustacheExpression expression, params AttributeNode[] childNodes)
        {
            Expression = expression;
            ChildNodes = childNodes;
        }

        public override void Accept(INodeVisitor visitor)
        {
            Expression.Accept(visitor);
            foreach (var child in ChildNodes)
                child.Accept(visitor);

            visitor.Visit(this);
        }
    }
}