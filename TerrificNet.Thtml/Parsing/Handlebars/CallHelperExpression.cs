namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public class CallHelperExpression : AccessExpression
    {
        public string Name { get; }
        public HelperAttribute[] Attributes { get; }

        public CallHelperExpression(string name, params HelperAttribute[] attributes)
        {
            Name = name;
            Attributes = attributes;
        }
    }
}