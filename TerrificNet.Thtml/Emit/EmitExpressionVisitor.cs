using System.Collections.Generic;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Emit
{
    public class EmitExpressionVisitor : ExpressionVisitor
    {
        private readonly Stack<DataBinderResult> _dataBinder = new Stack<DataBinderResult>();
        private DataBinderResult Scope => _dataBinder.Peek();

        public DataBinderResult Value { get; private set; }

        public EmitExpressionVisitor(IDataBinder dataBinder)
        {
            _dataBinder.Push(dataBinder.Context());
            Value = Scope;
        }

        public override bool BeforeVisit(IterationExpression iterationExpression)
        {

            return true;
        }

        public override void AfterVisit(IterationExpression iterationExpression)
        {
            _dataBinder.Push(Value.Item());
        }

        public override bool BeforeVisit(MemberExpression memberExpression)
        {
            Value = Scope.Evaluate(memberExpression.Name);
            return false;
        }

        public void AfterStatement()
        {
            _dataBinder.Pop();
        }
    }
}