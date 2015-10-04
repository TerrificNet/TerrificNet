namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class CallHelperExpression : MustacheExpression
    {
        public string Name { get; }
        public HelperAttribute[] Attributes { get; }

        public CallHelperExpression(string name, params HelperAttribute[] attributes)
        {
            Name = name;
            Attributes = attributes;
        }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}