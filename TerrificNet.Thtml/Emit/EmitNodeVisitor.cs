using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class PropertyValueEmitter : NodeVisitorBase<IEmitterRunnable<VPropertyValue>>
    {
        private readonly IDataBinder _dataBinder;

        public PropertyValueEmitter(IDataBinder dataBinder)
        {
            _dataBinder = dataBinder;
        }

        public override IEmitterRunnable<VPropertyValue> Visit(AttributeContentStatement constantAttributeContent)
        {
            return constantAttributeContent.Expression.Accept(this);
        }

        public override IEmitterRunnable<VPropertyValue> Visit(MemberExpression memberExpression)
        {
            var scope = GetScope(memberExpression, _dataBinder);

            IEvaluator<string> evaluator;
            if (!scope.TryCreateEvaluation(out evaluator))
                throw new Exception();

            return EmitterNode.Lambda((d, r) => new StringVPropertyValue(evaluator.Evaluate(d)));
        }

        public static IDataBinder GetScope(MemberExpression memberExpression, IDataBinder parentScope)
        {
            var expression = memberExpression;

            while (expression != null)
            {
                parentScope = parentScope.Property(expression.Name);
                expression = expression.SubExpression;
            }
            return parentScope;
        }

        public override IEmitterRunnable<VPropertyValue> Visit(ConstantAttributeContent attributeContent)
        {
            return EmitterNode.Lambda((d, r) => new StringVPropertyValue(attributeContent.Text));
        }

        private static VPropertyValue GetPropertyValue(IListEmitter<VPropertyValue> emitter, IDataContext dataContext, IRenderingContext renderingContext)
        {
            var stringBuilder = new StringBuilder();
            foreach (var emit in emitter.Execute(dataContext, renderingContext))
            {
                var stringValue = emit as StringVPropertyValue;
                if (stringValue == null)
                    throw new Exception($"Unsupported property value {emit.GetType()}.");

                stringBuilder.Append(stringValue.Value);
            }

            return new StringVPropertyValue(stringBuilder.ToString());
        }
    }

    internal class PropertyEmitter : NodeVisitorBase<IListEmitter<VProperty>>
    {
        private readonly IDataBinder _dataBinder;

        public PropertyEmitter(IDataBinder dataBinder)
        {
            _dataBinder = dataBinder;
        }

        public override IListEmitter<VProperty> Visit(AttributeNode attributeNode)
        {
            var valueVisitor = new PropertyValueEmitter(_dataBinder);
            var valueEmitter = attributeNode.Value.Accept(valueVisitor);

            if (valueEmitter == null)
                valueEmitter = EmitterNode.Lambda<VPropertyValue>((d, r) => null);

            return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VProperty(attributeNode.Name, valueEmitter.Execute(d, r))));
        }
    }

	internal class EmitNodeVisitor : NodeVisitorBase<IListEmitter<VTree>>
	{
		private readonly IHelperBinder _helperBinder;
		//private readonly EmitExpressionVisitor _expressionVisitor;
		private readonly Stack<List<IListEmitter<VTree>>> _elements = new Stack<List<IListEmitter<VTree>>>();
		private readonly Stack<IDataBinder> _dataBinderStack = new Stack<IDataBinder>();
		private readonly Stack<List<IListEmitter<VProperty>>> _properties = new Stack<List<IListEmitter<VProperty>>>();

		private List<IListEmitter<VTree>> Scope => _elements.Peek();
		private IDataBinder Value { get; set; }

		public EmitNodeVisitor(IDataBinder dataBinder, IHelperBinder helperBinder)
		{
			_helperBinder = helperBinder;
			_dataBinderStack.Push(dataBinder);
		}

		public IEmitterRunnable<VNode> DocumentFunc { get; private set; }

		public override IListEmitter<VTree> Visit(Element element)
		{
		    var attributeVisitor = new PropertyEmitter(_dataBinderStack.Peek());

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
			var contentEmitter = statement.Expression.Accept(this);
            if (contentEmitter != null)
                return contentEmitter;

            var elements = statement.ChildNodes.Select(childNode => childNode.Accept(this)).ToList();
            return EmitterNode.Many(elements);
		}

		private void EnterScope()
		{
			_elements.Push(new List<IListEmitter<VTree>>());
		}

		private List<IListEmitter<VTree>> LeaveScope()
		{
			return _elements.Pop();
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
			unconvertedExpression.Accept(this);

            return null;
        }

	    public override IListEmitter<VTree> Visit(IterationExpression iterationExpression)
		{
		    var expression = iterationExpression.Expression;
		    expression.Accept(this);
			_dataBinderStack.Push(Value.Item());

            return null;
        }

		public override IListEmitter<VTree> Visit(ConditionalExpression conditionalExpression)
		{
			EnterScope();
			conditionalExpression.Expression.Accept(this);
			var scope = LeaveScope();

			IEvaluator<bool> evaluator;
			if (!TryGetEvaluator(conditionalExpression.Expression, out evaluator))
				throw new Exception("Expect a boolean as result");

			//Scope.Add(EmitterNode.Condition(d => evaluator.Evaluate(d), EmitterNode.Many(scope)));

            return null;
        }

		public override IListEmitter<VTree> Visit(MemberExpression memberExpression)
		{
		    var scope = PropertyValueEmitter.GetScope(memberExpression, _dataBinderStack.Peek());

            IEvaluator<string> evaluator;
			if (scope.TryCreateEvaluation(out evaluator))
				return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText(evaluator.Evaluate(d))));

		    throw new Exception("no valid");
		}

		private bool TryGetEvaluator<T>(MustacheExpression expression, out IEvaluator<T> evaluator)
		{
			if (!Value.TryCreateEvaluation(out evaluator))
				return false;

			evaluator = ExceptionDecorator(evaluator, expression);
			return true;
		}

		private IListEmitter<VPropertyValue> TryConvertStringToVPropertyValue(MustacheExpression expression)
		{
			IEvaluator<string> evaluator;
			if (!TryGetEvaluator(expression, out evaluator))
				return null;

			return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new StringVPropertyValue(evaluator.Evaluate(d))));
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

    internal class NodeVisitorBase<T> : INodeVisitor<T>
    {
        public virtual T Visit(Element element)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(TextNode textNode)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(Statement statement)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(AttributeNode attributeNode)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(AttributeContentStatement constantAttributeContent)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(ConstantAttributeContent attributeContent)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(Document document)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(CompositeAttributeContent compositeAttributeContent)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(CallHelperExpression callHelperExpression)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(UnconvertedExpression unconvertedExpression)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(AttributeStatement attributeStatement)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(IterationExpression iterationExpression)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(ConditionalExpression conditionalExpression)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(MemberExpression memberExpression)
        {
            throw new NotImplementedException();
        }
    }

}