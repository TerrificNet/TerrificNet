using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class EmitNodeVisitor : NodeVisitorBase<IListEmitter<VTree>>
	{
        private readonly IHelperBinder _helperBinder;
        private readonly IDataBinder _dataBinder;

        public EmitNodeVisitor(IDataBinder dataBinder, IHelperBinder helperBinder)
        {
            _helperBinder = helperBinder;
            _dataBinder = dataBinder;
        }

        public IEmitterRunnable<VNode> DocumentFunc { get; private set; }

		public override IListEmitter<VTree> Visit(Element element)
		{
		    var attributeVisitor = new PropertyEmitter(_dataBinder);

		    var properties = element.Attributes.Select(attribute => attribute.Accept(attributeVisitor)).ToList();
		    var elements = element.ChildNodes.Select(node => node.Accept(this)).ToList();

		    var emitter = EmitterNode.Many(elements);
			var attributeEmitter = EmitterNode.Many(properties);

            return EmitterNode.AsList(
                EmitterNode.Lambda((d, r) => new VElement(element.TagName, attributeEmitter.Execute(d, r), emitter.Execute(d, r))));
		}

        public override IListEmitter<VTree> Visit(TextNode textNode)
		{
			return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText(textNode.Text)));
		}

        public override IListEmitter<VTree> Visit(Statement statement)
        {
            var expression = statement.Expression;
            var iterationExpression = expression as IterationExpression;
            var dataBinder = _dataBinder;
            if (iterationExpression != null)
            {
                var scope = ScopeEmitter.Bind(dataBinder, iterationExpression.Expression);

                IEvaluator<IEnumerable> evaluator;
                if (!scope.TryCreateEvaluation(out evaluator))
                    throw new Exception("Expect a enumerable as result");

                var child = new EmitNodeVisitor(scope.Item(), _helperBinder);
                var children = statement.ChildNodes.Select(c => c.Accept(child)).ToList();

                return EmitterNode.Iterator(d => evaluator.Evaluate(d), EmitterNode.Many(children));
            }

            var conditionalExpression = expression as ConditionalExpression;
            if (conditionalExpression != null)
            {
                var scope = ScopeEmitter.Bind(dataBinder, conditionalExpression.Expression);

                IEvaluator<bool> evaluator;
                if (!scope.TryCreateEvaluation(out evaluator))
                    throw new Exception("Expect a boolean as result");

                var children = statement.ChildNodes.Select(c => c.Accept(this)).ToList();
                return EmitterNode.Condition(d => evaluator.Evaluate(d), EmitterNode.Many(children));
            }

            var callHelperExpression = expression as CallHelperExpression;
            if (callHelperExpression != null)
            {
                var result = _helperBinder.FindByName(callHelperExpression.Name, CreateDictionaryFromArguments(callHelperExpression.Attributes));
                if (result == null)
                    throw new Exception($"Unknown helper with name {callHelperExpression.Name}.");

                var children = statement.ChildNodes.Select(c => c.Accept(this)).ToList();
                var evaluation = result.CreateEmitter(EmitterNode.Many(children), _helperBinder, dataBinder);
                return evaluation;
            }

            var contentEmitter = statement.Expression.Accept(this);
            if (contentEmitter != null)
                return contentEmitter;

            var elements = statement.ChildNodes.Select(childNode => childNode.Accept(this)).ToList();
            return EmitterNode.Many(elements);
		}

        public override IListEmitter<VTree> Visit(Document document)
		{
		    var elements = document.ChildNodes.Select(node => node.Accept(this)).ToList();

		    var emitter = EmitterNode.Many(elements);
			DocumentFunc = EmitterNode.Lambda((d, r) => new VNode(emitter.Execute(d, r)));

            return emitter;
        }

        public override IListEmitter<VTree> Visit(UnconvertedExpression unconvertedExpression)
		{
			return unconvertedExpression.Accept(this);
		}

        private IDictionary<string, string> CreateDictionaryFromArguments(HelperAttribute[] attributes)
        {
            return attributes.ToDictionary(d => d.Name, d => d.Value);
        }

		public override IListEmitter<VTree> Visit(MemberExpression memberExpression)
		{
		    var scope = ScopeEmitter.Bind(_dataBinder, memberExpression);

            IEvaluator<string> evaluator;
			if (scope.TryCreateEvaluation(out evaluator))
				return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText(evaluator.Evaluate(d))));

		    throw new Exception("no valid");
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