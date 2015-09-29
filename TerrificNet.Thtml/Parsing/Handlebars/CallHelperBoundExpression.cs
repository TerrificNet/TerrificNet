namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class CallHelperBoundExpression : AccessExpression
    {
        public string Name { get; }
        public MemberAccessExpression AccessExpression { get; }

        public CallHelperBoundExpression(string name, MemberAccessExpression accessExpression)
        {
            Name = name;
            AccessExpression = accessExpression;
        }
    }
}