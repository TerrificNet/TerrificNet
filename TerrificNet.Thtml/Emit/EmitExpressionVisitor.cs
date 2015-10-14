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
            IEvaluater<string> evalutor;
            if (!TryGetEvalutor(expression, out evalutor))
                return null;

            return EmitterNode.AsList(EmitterNode.Lambda(d => new StringVPropertyValue(evalutor.Evaluate(d))));
        }

        private bool TryGetEvalutor<T>(MustacheExpression expression, out IEvaluater<T> evalutor)
        {
            if (!Value.TryCreateEvaluation(out evalutor))
                return false;

            evalutor = ExceptionDecorator(evalutor, expression);
            return true;
        }

        private IListEmitter<T> LeaveScope<T>(MustacheExpression expression, IListEmitter<T> children, params Func<MustacheExpression, IListEmitter<T>>[] converters)
        {
            _dataBinder.Pop();

            var iterationExpression = expression as IterationExpression;
            if (iterationExpression != null)
            {
                iterationExpression.Expression.Accept(this);

                IEvaluater<IEnumerable> evalutor;
                if (!TryGetEvalutor(expression, out evalutor))
                    throw new Exception("Expect a enumerable as result");

                return EmitterNode.Iterator(d => evalutor.Evaluate(d), children);                
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
            IEvaluater<VText> evalutor;
            if (!TryGetEvalutor(expression, out evalutor))
                return null;

            return EmitterNode.AsList(EmitterNode.Lambda(d => evalutor.Evaluate(d)));
        }

        private IListEmitter<VText> TryConvertStringToVText(MustacheExpression expression)
        {
            IEvaluater<string> evalutor;
            if (!TryGetEvalutor(expression, out evalutor))
                return null;

            return EmitterNode.AsList(EmitterNode.Lambda(d => new VText(evalutor.Evaluate(d))));
        }

        private static IEvaluater<T> ExceptionDecorator<T>(IEvaluater<T> createEvaluation, MustacheExpression expression)
        {
            return new ExceptionWrapperEvalutor<T>(createEvaluation, expression);
        }

        private class ExceptionWrapperEvalutor<T> : IEvaluater<T>
        {
            private readonly IEvaluater<T> _evaluator;
            private readonly MustacheExpression _expression;

            public ExceptionWrapperEvalutor(IEvaluater<T> evaluator, MustacheExpression expression)
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