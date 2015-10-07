using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    public class EmitExpressionVisitor : ExpressionVisitor
    {
        private readonly Stack<DataBinderResult> _dataBinder = new Stack<DataBinderResult>();
        private DataBinderResult Scope => _dataBinder.Peek();

        private DataBinderResult Value { get; set; }

        public EmitExpressionVisitor(IDataBinder dataBinder)
        {
            _dataBinder.Push(dataBinder.Context());
            Value = Scope;
        }

        public override bool BeforeVisit(MemberExpression memberExpression)
        {
            if (memberExpression.Name == "this")
                Value = Scope;
            else
                Value = Scope.Evaluate(memberExpression.Name);

            return false;
        }

        public void EnterScope(MustacheExpression expression)
        {
            var iterationExpression = expression as IterationExpression;
            if (iterationExpression != null)
                expression = iterationExpression.Expression;

            expression.Accept(this);
            _dataBinder.Push(Value.Item());
        }

        public IListEmitter<VTree> LeaveScope(MustacheExpression expression, IListEmitter<VTree> children)
        {
            _dataBinder.Pop();

            var iterationExpression = expression as IterationExpression;
            if (iterationExpression != null)
            {
                iterationExpression.Expression.Accept(this);
                if (Value.ResultType.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var evaluation = Wrap(Value.CreateEvaluation<IEnumerable>(), expression);
                    return EmitterNode.Iterator(d => evaluation(d), children);
                }

                throw new Exception("Expect a enumerable as result");
            }

            if (Value.ResultType == typeof(string))
            {
                var evaluation = Wrap(Value.CreateEvaluation<string>(), expression);
                return EmitterNode.AsList(EmitterNode.Lambda(d => new VText(evaluation(d))));
            }

            if (Value.ResultType == typeof(VText))
            {
                return EmitterNode.AsList(EmitterNode.Lambda(Wrap(Value.CreateEvaluation<VText>(), expression)));
            }

            throw new Exception("Expect a VText or string as result");
        }

        private static Func<IDataContext, T> Wrap<T>(Func<IDataContext, T> createEvaluation, MustacheExpression expression)
        {
            return c =>
            {
                try
                {
                    return createEvaluation(c);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Exception on executing expression {expression}", ex);
                }
            };
        }
    }
}