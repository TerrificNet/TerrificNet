namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class IterationExpression : AccessExpression
    {
        public MemberAccessExpression Expression { get; }

        public IterationExpression(MemberAccessExpression expression)
        {
            Expression = expression;
        }
    }
}