namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class MemberExpression : MustacheExpression
    {
        public string Name { get; }

        public MustacheExpression SubExpression { get; }

        public MemberExpression(string name)
        {
            Name = name;
        }

        public MemberExpression(string name, MustacheExpression subExpression)
            : this(name)
        {
            SubExpression = subExpression;
        }

        public override void Accept(IExpressionVisitor visitor)
        {
            if (!visitor.BeforeVisit(this))
                return;

            SubExpression?.Accept(visitor);

            visitor.AfterVisit(this);
        }
    }
}