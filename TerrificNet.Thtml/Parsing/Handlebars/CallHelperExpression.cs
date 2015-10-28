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

	    public override T Accept<T>(INodeVisitor<T> visitor)
	    {
		    return visitor.Visit(this);
	    }
    }
}