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
        private readonly IHelperBinder _helperBinder;
        private readonly Stack<DataBinderResult> _dataBinder = new Stack<DataBinderResult>();
        private DataBinderResult Scope => _dataBinder.Peek();

        private DataBinderResult Value { get; set; }

        public EmitExpressionVisitor(IDataBinder dataBinder, IHelperBinder helperBinder)
        {
            _helperBinder = helperBinder;
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
            {
                expression = iterationExpression.Expression;

                expression.Accept(this);
                _dataBinder.Push(Value.Item());
            }
            else
            {
                expression.Accept(this);
                _dataBinder.Push(Value);
            }
        }

        public IListEmitter<VTree> LeaveTreeScope(MustacheExpression expression, IListEmitter<VTree> children)
        {
            return LeaveScope(expression, children, TryConvertStringToVText, TryConvertVText);
        }

        public IListEmitter<VPropertyValue> LeavePropertyValueScope(MustacheExpression expression)
        {
            return LeaveScope(expression, null, TryConvertStringToVPropertyValue);
        }

        private IListEmitter<VPropertyValue> TryConvertStringToVPropertyValue(MustacheExpression expression)
        {
            if (Value.ResultType != typeof(string))
                return null;

            var evaluation = Wrap(Value.CreateEvaluation<string>(), expression);
            return EmitterNode.AsList(EmitterNode.Lambda(d => new StringVPropertyValue(evaluation(d))));
        }

        private IListEmitter<T> LeaveScope<T>(MustacheExpression expression, IListEmitter<T> children, params Func<MustacheExpression, IListEmitter<T>>[] converters)
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

            var callHelperExpression = expression as CallHelperExpression;
            if (callHelperExpression != null)
            {
                var result = _helperBinder.FindByName(callHelperExpression.Name, CreateDictionaryFromArguments(callHelperExpression.Attributes));
                if (result == null)
                    throw new Exception($"Unknown helper with name {callHelperExpression.Name}.");

                var evalutation = result.CreateEmitter(children, _helperBinder, Scope);
                return evalutation;
            }

            foreach (var converter in converters)
            {
                IListEmitter<T> leaveScope;
                if ((leaveScope = converter(expression)) != null)
                    return leaveScope;
            }

            throw new Exception("Expect a VText or string as result");
        }

        private IDictionary<string, string> CreateDictionaryFromArguments(HelperAttribute[] attributes)
        {
            return attributes.ToDictionary(d => d.Name, d => d.Value);
        }

        private IListEmitter<VText> TryConvertVText(MustacheExpression expression)
        {
            if (Value.ResultType != typeof(VText))
                return null;

            return EmitterNode.AsList(EmitterNode.Lambda(Wrap(Value.CreateEvaluation<VText>(), expression)));
        }

        private IListEmitter<VText> TryConvertStringToVText(MustacheExpression expression)
        {
            if (Value.ResultType != typeof (string))
                return null;

            var evaluation = Wrap(Value.CreateEvaluation<string>(), expression);
            return EmitterNode.AsList(EmitterNode.Lambda(d => new VText(evaluation(d))));
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