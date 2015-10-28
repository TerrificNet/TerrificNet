using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Emit
{
    internal class ScopeEmitter : NodeVisitorBase<IDataBinder>
    {
        private IDataBinder _dataBinder;

        private ScopeEmitter(IDataBinder dataBinder)
        {
            _dataBinder = dataBinder;
        }

        public override IDataBinder Visit(MemberExpression memberExpression)
        {
            _dataBinder = _dataBinder.Property(memberExpression.Name);
            if (memberExpression.SubExpression != null)
                return memberExpression.SubExpression.Accept(this);

            return _dataBinder;
        }

        public static IDataBinder Bind(IDataBinder binder, MustacheExpression expression)
        {
            var visitor = new ScopeEmitter(binder);
            return expression.Accept(visitor);
        }
    }
}