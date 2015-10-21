namespace TerrificNet.Thtml.Parsing
{
    public interface INodeVisitor
    {
        void Visit(SyntaxNode node);
    }
}