namespace TerrificNet.Thtml.Parsing
{
    public abstract class Node : HtmlNode
    {
    }

    public abstract class HtmlNode : SyntaxNode
    {
        public abstract void Accept(INodeVisitor visitor);
    }
}