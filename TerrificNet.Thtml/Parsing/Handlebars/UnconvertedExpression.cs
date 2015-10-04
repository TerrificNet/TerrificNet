namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class UnconvertedExpression : MustacheExpression
    {
        public MustacheExpression Expression { get; set; }

        public UnconvertedExpression(MustacheExpression expression)
        {
            Expression = expression;
        }

        public override void Accept(INodeVisitor visitor)
        {
            Expression.Accept(visitor);
            visitor.Visit(this);
        }
    }
}