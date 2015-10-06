namespace TerrificNet.Thtml.Parsing.Handlebars
{
    public interface IExpressionVisitor
    {
        void Visit(CallHelperExpression callHelperExpression);

        bool BeforeVisit(ConditionalExpression conditionalExpression);
        void AfterVisit(ConditionalExpression conditionalExpression);
        bool BeforeVisit(IterationExpression iterationExpression);
        void AfterVisit(IterationExpression iterationExpression);
        bool BeforeVisit(MemberExpression memberExpression);
        void AfterVisit(MemberExpression memberExpression);
        bool BeforeVisit(UnconvertedExpression unconvertedExpression);
        void AfterVisit(UnconvertedExpression unconvertedExpression);

    }
}