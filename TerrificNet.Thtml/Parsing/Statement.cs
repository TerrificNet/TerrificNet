using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class Statement : Node
    {
        public Statement(MustacheExpression expression, params Node[] childNodes)
        {
            Expression = expression;
            ChildNodes = childNodes;
        }

        public MustacheExpression Expression { get; }

        public Node[] ChildNodes { get; }
        public override void Accept(INodeVisitor visitor)
        {
            if (!visitor.BeforeVisit(this))
                return;

            foreach (var child in ChildNodes)
                child.Accept(visitor);

            visitor.AfterVisit(this);
        }
    }
}