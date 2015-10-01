namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class MemberExpression : Expression
    {
        public string Name { get; }

        public Expression SubExpression { get; }

        public MemberExpression(string name)
        {
            Name = name;
        }

        public MemberExpression(string name, Expression subExpression)
            : this(name)
        {
            SubExpression = subExpression;
        }
    }
}