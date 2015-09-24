namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class ConditionalExpression : AccessExpression
    {
        public MemberAccessExpression MemberAccessExpression { get; }

        public ConditionalExpression(MemberAccessExpression memberAccessExpression)
        {
            MemberAccessExpression = memberAccessExpression;
        }
    }
}