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
	internal class EmitNodeVisitor : INodeVisitor
	{
		private readonly IHelperBinder _helperBinder;
		//private readonly EmitExpressionVisitor _expressionVisitor;
		private readonly Stack<List<IListEmitter<VTree>>> _elements = new Stack<List<IListEmitter<VTree>>>();
		private readonly Stack<IDataBinder> _dataBinderStack = new Stack<IDataBinder>();
		private readonly Stack<List<IListEmitter<VProperty>>> _properties = new Stack<List<IListEmitter<VProperty>>>();
		private IEmitterRunnable<VPropertyValue> _propertyValueEmitter;
		private IListEmitter<VPropertyValue> _propertyEmitter;

		private List<IListEmitter<VTree>> Scope => _elements.Peek();
		private IDataBinder Value { get; set; }

		public EmitNodeVisitor(IDataBinder dataBinder, IHelperBinder helperBinder)
		{
			_helperBinder = helperBinder;
			_dataBinderStack.Push(dataBinder);
		}

		public IEmitterRunnable<VNode> DocumentFunc { get; private set; }

		public void Visit(Element element)
		{
			EnterScope();
			_properties.Push(new List<IListEmitter<VProperty>>());

			foreach (var attribute in element.Attributes)
			{
				attribute.Accept(this);
			}
			foreach (var node in element.ChildNodes)
			{
				node.Accept(this);
			}

			var emitter = EmitterNode.Many(LeaveScope());
			var attributeEmitter = EmitterNode.Many(_properties.Pop());
			Scope.Add(EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VElement(element.TagName, attributeEmitter.Execute(d, r), emitter.Execute(d, r)))));
		}

		public void Visit(TextNode textNode)
		{
			Scope.Add(EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText(textNode.Text))));
		}

		public void Visit(Statement statement)
		{
			EnterScope();
			statement.Expression.Accept(this); // Set data scopes
			
			foreach (var childNode in statement.ChildNodes)
			{
				childNode.Accept(this);
			}

			var childEmitter = EmitterNode.Many(LeaveScope());
			_dataBinderStack.Pop();

			//var listEmitter = _expressionVisitor.GetEmitter();
			//var listEmitter = _expressionVisitor.LeaveTreeScope(statement.Expression, childEmitter);
			IEvaluator<IEnumerable> evaluator;
			if (TryGetEvaluator(statement.Expression, out evaluator))
			{
				Scope.Add(EmitterNode.Iterator(d => evaluator.Evaluate(d), childEmitter));
			}
			else
			{
				Scope.Add(childEmitter);
			}
		}

		public void Visit(AttributeNode attributeNode)
		{
			attributeNode.Value.Accept(this);

			if (_propertyValueEmitter == null)
				_propertyValueEmitter = EmitterNode.Lambda<VPropertyValue>((d, r) => null);

			var valueEmitter = _propertyValueEmitter;
			_properties.Peek().Add(EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VProperty(attributeNode.Name, valueEmitter.Execute(d, r)))));

			_propertyValueEmitter = null;
		}

		public void Visit(AttributeContentStatement constantAttributeContent)
		{
			constantAttributeContent.Expression.Accept(this);
			//var emitter = _expressionVisitor.LeavePropertyValueScope(constantAttributeContent.Expression);
			//constantAttributeContent.Expression.AcceptLeave(_expressionVisitor, null);
			//var emitter = _expressionVisitor.GetPropertyValueEmitter();

			_propertyValueEmitter = EmitterNode.Lambda((d, r) => GetPropertyValue(_propertyEmitter, d, r));
		}

		public void Visit(ConstantAttributeContent attributeContent)
		{
			_propertyValueEmitter = EmitterNode.Lambda((d, r) => new StringVPropertyValue(attributeContent.Text));
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

		private void EnterScope()
		{
			_elements.Push(new List<IListEmitter<VTree>>());
		}

		private List<IListEmitter<VTree>> LeaveScope()
		{
			return _elements.Pop();
		}

		public void Visit(Document document)
		{
			EnterScope();

			foreach (var node in document.ChildNodes)
			{
				node.Accept(this);
			}

			var emitter = EmitterNode.Many(LeaveScope());
			DocumentFunc = EmitterNode.Lambda((d, r) => new VNode(emitter.Execute(d, r)));
		}

		public void Visit(CallHelperExpression callHelperExpression)
		{

		}

		public void Visit(UnconvertedExpression unconvertedExpression)
		{
			unconvertedExpression.Accept(this);
		}

		public void Visit(IterationExpression iterationExpression)
		{
			iterationExpression.Expression.Accept(this);
			_dataBinderStack.Push(Value.Item());
		}

		public void Visit(ConditionalExpression conditionalExpression)
		{
			EnterScope();
			conditionalExpression.Expression.Accept(this);
			var scope = LeaveScope();

			IEvaluator<bool> evaluator;
			if (!TryGetEvaluator(conditionalExpression.Expression, out evaluator))
				throw new Exception("Expect a boolean as result");

			Scope.Add(EmitterNode.Condition(d => evaluator.Evaluate(d), EmitterNode.Many(scope)));
		}

		public void Visit(MemberExpression memberExpression)
		{
			var binder = _dataBinderStack.Peek();
			Value = memberExpression.Name == "this" ? binder : binder.Property(memberExpression.Name);

			IEvaluator<string> evaluator;
			if (TryGetEvaluator(memberExpression, out evaluator))
			{
				var emitter = EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText(evaluator.Evaluate(d))));
				Scope.Add(emitter);
				//EmitterNode.Lambda((d, r) => new StringVPropertyValue(evaluator.Evaluate(d)));
			}
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
}