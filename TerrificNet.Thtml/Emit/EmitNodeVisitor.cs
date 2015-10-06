using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class EmitNodeVisitor : NodeVisitor
    {
        private readonly EmitExpressionVisitor _expressionVisitor;
        private readonly List<IListEmitter<VTree>> _elements = new List<IListEmitter<VTree>>();

        public EmitNodeVisitor(IDataBinder dataBinder) : this(new EmitExpressionVisitor(dataBinder))
        {
        }

        private EmitNodeVisitor(EmitExpressionVisitor expressionVisitor) : base(expressionVisitor)
        {
            _expressionVisitor = expressionVisitor;
        }

        public IEmitter<VNode> DocumentFunc { get; private set; }

        public override bool BeforeVisit(Element element)
        {
            return true;
        }

        public override void AfterVisit(Element element)
        {
            var children = _elements.ToList();
            _elements.Clear();
            var emitter = EmitterNode.Many(children);
            _elements.Add(EmitterNode.AsList(EmitterNode.Lambda(d => new VElement(element.TagName, emitter.Execute(d)))));
        }

        public override void Visit(TextNode textNode)
        {
            _elements.Add(EmitterNode.AsList(EmitterNode.Lambda(d => new VText(textNode.Text))));
        }

        public override bool BeforeVisit(Document document)
        {
            return true;
        }

        public override void AfterVisit(Document document)
        {
            var emitter = EmitterNode.Many(_elements);
            DocumentFunc = EmitterNode.Lambda(d => new VNode(emitter.Execute(d)));
        }

        public override bool BeforeVisit(Statement statement)
        {
            statement.Expression.Accept(_expressionVisitor);
            return true;
        }

        public override void AfterVisit(Statement statement)
        {
            IListEmitter<VTree> listEmitter;
            if (statement.ChildNodes.Length == 0)
            {
                listEmitter = EmitterNode.AsList(EmitterNode.Lambda(GetEvaluationForVText(_expressionVisitor.Value, statement.Expression)));
            }
            else
            {
                statement.Expression.Accept(_expressionVisitor);
                _expressionVisitor.AfterStatement();

                var children = _elements.ToList();
                _elements.Clear();
                var emitter = EmitterNode.Many(children);
                var iteratorStatement = GetEvaluationForIteration(_expressionVisitor.Value, statement.Expression);
                listEmitter = EmitterNode.Iterator(iteratorStatement, emitter);
            }
            _elements.Add(listEmitter);
        }

        private static Func<IDataContext, IEnumerable> GetEvaluationForIteration(DataBinderResult dataBinderResult, MustacheExpression expression)
        {
            if (dataBinderResult.ResultType.GetInterfaces().Contains(typeof (IEnumerable)))
            {
                var evaluation = Wrap(dataBinderResult.CreateEvaluation<IEnumerable>(), expression);
                return d => evaluation(d);
            }

            throw new Exception("Expect a enumerable as result");
        }

        private static Func<IDataContext, VText> GetEvaluationForVText(DataBinderResult dataBinderResult, MustacheExpression expression)
        {
            if (dataBinderResult.ResultType == typeof (string))
            {
                var evaluation = Wrap(dataBinderResult.CreateEvaluation<string>(), expression);
                return d => new VText(evaluation(d));
            }

            if (dataBinderResult.ResultType == typeof (VText))
            {
                return Wrap(dataBinderResult.CreateEvaluation<VText>(), expression);
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