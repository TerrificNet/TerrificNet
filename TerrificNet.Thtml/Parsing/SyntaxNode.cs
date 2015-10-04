namespace TerrificNet.Thtml.Parsing
{
    public abstract class SyntaxNode
    {
        public string TypeName => GetType().Name;

        public abstract void Accept(INodeVisitor visitor);
    }
}