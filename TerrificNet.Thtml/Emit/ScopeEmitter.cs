using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Emit
{
    internal class ScopeEmitter : NodeVisitorBase<IDataScope>
    {
        private IDataScope _dataScope;

        private ScopeEmitter(IDataScope dataScope)
        {
            _dataScope = dataScope;
        }

        public override IDataScope Visit(MemberExpression memberExpression)
        {
            _dataScope = _dataScope.Property(memberExpression.Name);
            if (memberExpression.SubExpression != null)
                return memberExpression.SubExpression.Accept(this);

            return _dataScope;
        }

        public static IDataScope Bind(IDataScope scope, MustacheExpression expression)
        {
            var visitor = new ScopeEmitter(scope);
            return expression.Accept(visitor);
        }
    }
}