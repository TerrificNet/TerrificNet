using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using ConditionalExpression = TerrificNet.Thtml.Parsing.Handlebars.ConditionalExpression;
using MemberExpression = TerrificNet.Thtml.Parsing.Handlebars.MemberExpression;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class EmitExpressionVisitor : NodeVisitorBase<Expression>
	{
		private readonly IDataScopeContract _dataScopeContract;
		private readonly IHelperBinder<Expression, ExpressionHelperConfig> _helperBinder;
		private readonly ParameterExpression _writerParameter;
		private readonly ParameterExpression _dataContextParameter;
		private readonly Handler _handler;

		public EmitExpressionVisitor(IDataScopeContract dataScopeContract, IHelperBinder<Expression, ExpressionHelperConfig> helperBinder, ParameterExpression dataContextParameter, ParameterExpression writerParameter)
		{
			_dataScopeContract = dataScopeContract;
			_helperBinder = helperBinder;
			_dataContextParameter = dataContextParameter;
			_writerParameter = writerParameter;
			_handler = new Handler(writerParameter);
		}

		public override Expression Visit(Document document)
		{
			return Many(document.ChildNodes.Select(node => node.Accept(this)).ToList());
		}

		public override Expression Visit(Element element)
		{
			var expressions = _handler.HandleElement(element, this);

			return Expression.Block(expressions);
		}

		public override Expression Visit(AttributeNode attributeNode)
		{
			var valueEmitter = attributeNode.Value.Accept(this);
			var expressions = _handler.HandleAttributeNode(attributeNode, valueEmitter);

			return Expression.Block(expressions);
		}

		public override Expression Visit(ConstantAttributeContent attributeContent)
		{
			return _handler.HandleAttributeContent(attributeContent);
		}

		public override Expression Visit(AttributeContentStatement constantAttributeContent)
		{
			return HandleStatement(constantAttributeContent.Expression, constantAttributeContent.Children);
		}

		public override Expression Visit(Statement statement)
		{
			var expression = statement.Expression;
			return HandleStatement(expression, statement.ChildNodes);
		}

		public override Expression Visit(UnconvertedExpression unconvertedExpression)
		{
			return unconvertedExpression.Expression.Accept(this);
		}

		public override Expression Visit(CompositeAttributeContent compositeAttributeContent)
		{
			return Many(compositeAttributeContent.ContentParts.Select(p => p.Accept(this)).ToList());
		}

		public override Expression Visit(MemberExpression memberExpression)
		{
			var scope = ScopeEmitter.Bind(_dataScopeContract, memberExpression);
			var binding = scope.RequiresString();

			var evaluator = binding.CreateEvaluator();
			var evaluateMethod = ExpressionHelper.GetMethodInfo<IEvaluator<string>>(i => i.Evaluate(null));
			var callExpression = Expression.Call(Expression.Constant(evaluator), evaluateMethod, _dataContextParameter);
			return _handler.HandleCall(callExpression);
		}

		public override Expression Visit(TextNode textNode)
		{
			return _handler.HandleTextNode(textNode);
		}

		private Expression HandleStatement(MustacheExpression expression, IEnumerable<HtmlNode> childNodes)
		{
			var iterationExpression = expression as IterationExpression;
			if (iterationExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, iterationExpression.Expression);

				IDataScopeContract childScopeContract;
				var binding = scope.RequiresEnumerable(out childScopeContract);
				var evaluator = binding.CreateEvaluator();

				var child = CreateVisitor(childScopeContract);
				var children = Many(childNodes.Select(c => c.Accept(child)).ToList());

				var evaluateMethod = ExpressionHelper.GetMethodInfo<IEvaluator<IEnumerable>>(i => i.Evaluate(null));
				var collection = Expression.Call(Expression.Constant(evaluator), evaluateMethod, _dataContextParameter);
				return ExpressionHelper.ForEach(collection, child._dataContextParameter, children);
			}

			var conditionalExpression = expression as ConditionalExpression;
			if (conditionalExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, conditionalExpression.Expression);
				var binding = scope.RequiresBoolean();
				var evaluator = binding.CreateEvaluator();

				var children = Many(childNodes.Select(c => c.Accept(this)).ToList());

				var evaluateMethod = ExpressionHelper.GetMethodInfo<IEvaluator<bool>>(i => i.Evaluate(null));
				var testExpression = Expression.Call(Expression.Constant(evaluator), evaluateMethod, _dataContextParameter);

				return Expression.IfThen(testExpression, children);
			}

			var callHelperExpression = expression as CallHelperExpression;
			if (callHelperExpression != null)
			{
				var result = _helperBinder.FindByName(callHelperExpression.Name,
					CreateDictionaryFromArguments(callHelperExpression.Attributes));
				if (result == null)
					throw new Exception($"Unknown helper with name {callHelperExpression.Name}.");

				var children = Many(childNodes.Select(c => c.Accept(this)).ToList());

				var evaluation = result.CreateEmitter(_handler, children, _helperBinder, _dataScopeContract);
				return evaluation;
			}

			var contentEmitter = expression.Accept(this);
			if (contentEmitter != null)
				return contentEmitter;

			return Many(childNodes.Select(childNode => childNode.Accept(this)).ToList());
		}

		private static Expression Many(IReadOnlyCollection<Expression> expressions)
		{
			return expressions.Count > 0 ? (Expression)Expression.Block(expressions) : Expression.Empty();
		}

		private static IDictionary<string, string> CreateDictionaryFromArguments(HelperAttribute[] attributes)
		{
			return attributes.ToDictionary(d => d.Name, d => d.Value);
		}

		private EmitExpressionVisitor CreateVisitor(IDataScopeContract childScopeContract)
		{
			var dataContextParameter = Expression.Parameter(childScopeContract.ResultType);
			return new EmitExpressionVisitor(childScopeContract, _helperBinder, dataContextParameter, _writerParameter);
		}	
	}

	public class Handler
	{
		private readonly ParameterExpression _writerParameter;

		public Handler(ParameterExpression writerParameter)
		{
			_writerParameter = writerParameter;
		}

		public Expression HandleAttributeContent(ConstantAttributeContent attributeContent)
		{
			return Write(attributeContent.Text);
		}

		internal IEnumerable<Expression> HandleElement(Element element, EmitExpressionVisitor visitor)
		{
			var expressions = new List<Expression>();
			expressions.Add(Write($"<{element.TagName}"));
			expressions.AddRange(element.Attributes.Select(attribute => attribute.Accept(visitor)));
			expressions.Add(Write(">"));
			expressions.AddRange(element.ChildNodes.Select(i => i.Accept(visitor)));
			expressions.Add(Write($"</{element.TagName}>"));
			return expressions;
		}

		public IEnumerable<Expression> HandleAttributeNode(AttributeNode attributeNode, Expression valueEmitter)
		{
			yield return Write(" " + attributeNode.Name + "=\"");
			yield return valueEmitter;
			yield return Write("\"");
		}

		public Expression HandleCall(Expression callExpression)
		{
			return ExpressionHelper.Write(_writerParameter, callExpression);
		}

		public Expression HandleTextNode(TextNode textNode)
		{
			return Write(textNode.Text);
		}

		private Expression Write(string value)
		{
			return ExpressionHelper.Write(_writerParameter, value);
		}
	}
}