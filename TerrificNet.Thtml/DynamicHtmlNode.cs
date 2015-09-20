namespace TerrificNet.Thtml
{
    public class DynamicHtmlNode : HtmlNode
    {
        public DynamicHtmlNode(string expression)
        {
            Expression = expression;
        }

        public string Expression { get; }
    }
}