namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public abstract class ExpressionVisitor : IExpressionVisitor
    {
        public virtual void Visit(CallHelperExpression callHelperExpression)
        {
        }

        public virtual bool BeforeVisit(ConditionalExpression conditionalExpression)
        {
            return false;
        }

        public virtual void AfterVisit(ConditionalExpression conditionalExpression)
        {
        }

        public virtual bool BeforeVisit(IterationExpression iterationExpression)
        {
            return false;
        }

        public virtual void AfterVisit(IterationExpression iterationExpression)
        {
        }

        public virtual bool BeforeVisit(MemberExpression memberExpression)
        {
            return false;
        }

        public virtual void AfterVisit(MemberExpression memberExpression)
        {
        }

        public virtual bool BeforeVisit(UnconvertedExpression unconvertedExpression)
        {
            return false;
        }

        public virtual void AfterVisit(UnconvertedExpression unconvertedExpression)
        {
        }
    }
}