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
        private readonly Stack<IDataBinder> _dataBinderStack = new Stack<IDataBinder>();
        private IDataBinder Scope => _dataBinderStack.Peek();

        private IDataBinder Value { get; set; }

        public EmitExpressionVisitor(IDataBinder dataBinder, IHelperBinder helperBinder)
        {
            _helperBinder = helperBinder;
            _dataBinderStack.Push(dataBinder.Context());
            Value = Scope;
        }

        public override bool BeforeVisit(MemberExpression memberExpression)
        {
            if (memberExpression.Name == "this")
                Value = Scope;
            else
                Value = Scope.Property(memberExpression.Name);

            return false;
        }

        public void EnterScope(MustacheExpression expression)
        {
            var iterationExpression = expression as IterationExpression;
            if (iterationExpression != null)
            {
                expression = iterationExpression.Expression;

                expression.Accept(this);
                _dataBinderStack.Push(Value.Item());
            }
            else
            {
                expression.Accept(this);
                //_dataBinder.Push(Value);
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
            IEvaluator<string> evaluator;
            if (!TryGetEvaluator(expression, out evaluator))
                return null;

            return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new StringVPropertyValue(evaluator.Evaluate(d))));
        }

        private bool TryGetEvaluator<T>(MustacheExpression expression, out IEvaluator<T> evaluator)
        {
            if (!Value.TryCreateEvaluation(out evaluator))
                return false;

            evaluator = ExceptionDecorator(evaluator, expression);
            return true;
        }

        private IListEmitter<T> LeaveScope<T>(MustacheExpression expression, IListEmitter<T> children, params Func<MustacheExpression, IListEmitter<T>>[] converters)
        {
            var iterationExpression = expression as IterationExpression;
            if (iterationExpression != null)
            {
                _dataBinderStack.Pop();
                iterationExpression.Expression.Accept(this);

                IEvaluator<IEnumerable> evaluator;
                if (!TryGetEvaluator(expression, out evaluator))
                    throw new Exception("Expect a enumerable as result");

                return EmitterNode.Iterator(d => evaluator.Evaluate(d), children);                
            }

            var conditionalExpression = expression as ConditionalExpression;
            if (conditionalExpression != null)
            {
                conditionalExpression.Expression.Accept(this);

                IEvaluator<bool> evaluator;
                if (!TryGetEvaluator(expression, out evaluator))
                    throw new Exception("Expect a boolean as result");

                return EmitterNode.Condition(d => evaluator.Evaluate(d), children);
            }

            var callHelperExpression = expression as CallHelperExpression;
            if (callHelperExpression != null)
            {
                var result = _helperBinder.FindByName(callHelperExpression.Name, CreateDictionaryFromArguments(callHelperExpression.Attributes));
                if (result == null)
                    throw new Exception($"Unknown helper with name {callHelperExpression.Name}.");

                var evaluation = result.CreateEmitter(children, _helperBinder, Scope);
                return evaluation;
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
            IEvaluator<VText> evaluator;
            if (!TryGetEvaluator(expression, out evaluator))
                return null;

            return EmitterNode.AsList(EmitterNode.Lambda((d, r) => evaluator.Evaluate(d)));
        }

        private IListEmitter<VText> TryConvertStringToVText(MustacheExpression expression)
        {
            IEvaluator<string> evaluator;
            if (!TryGetEvaluator(expression, out evaluator))
                return null;

            return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText(evaluator.Evaluate(d))));
        }

        private static IEvaluator<T> ExceptionDecorator<T>(IEvaluator<T> createEvaluation, MustacheExpression expression)
        {
            return new ExceptionWrapperEvaluator<T>(createEvaluation, expression);
        }

        private class ExceptionWrapperEvaluator<T> : IEvaluator<T>
        {
            private readonly IEvaluator<T> _evaluator;
            private readonly MustacheExpression _expression;

            public ExceptionWrapperEvaluator(IEvaluator<T> evaluator, MustacheExpression expression)
            {
                _evaluator = evaluator;
                _expression = expression;
            }

            public T Evaluate(IDataContext context)
            {
                try
                {
                    return _evaluator.Evaluate(context);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Exception on executing expression {_expression}", ex);
                }
            }
        }
    }
}