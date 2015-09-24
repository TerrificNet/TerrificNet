namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class AccessExpression
    {
        
    }

    public class MemberAccessExpression : AccessExpression
    {
        public string Name { get; }
        public MemberAccessExpression Expression { get; }

        public MemberAccessExpression(string name)
        {
            Name = name;
        }

        public MemberAccessExpression(string name, MemberAccessExpression expression) 
            : this(name)
        {
            Expression = expression;
        }
    }
}