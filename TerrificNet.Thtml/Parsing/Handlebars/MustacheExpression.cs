namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public abstract class MustacheExpression : SyntaxNode
    {
        public abstract void Accept(IExpressionVisitor visitor);
    }
}