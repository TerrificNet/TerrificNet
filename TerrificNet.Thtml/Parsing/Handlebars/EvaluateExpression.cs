namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class EvaluateExpression
    {
        public MemberAccessExpression Expression { get; }

        public EvaluateExpression(MemberAccessExpression expression)
        {
            Expression = expression;
        }
    }
}