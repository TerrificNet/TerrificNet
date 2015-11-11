using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using ConditionalExpression = TerrificNet.Thtml.Parsing.Handlebars.ConditionalExpression;
using MemberExpression = TerrificNet.Thtml.Parsing.Handlebars.MemberExpression;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class EmitExpressionVisitor : NodeVisitorBase<Expression>
	{
		private readonly IDataScopeContract _dataScopeContract;
		private readonly IHelperBinder _helperBinder;
		private readonly IDataBinder _dataBinder;
		private readonly ParameterExpression _dataContextParameter;
		private readonly IOutputExpressionEmitter _outputExpressionEmitter;

		public EmitExpressionVisitor(IDataScopeContract dataScopeContract, IHelperBinder helperBinder, IDataBinder dataBinder, ParameterExpression dataContextParameter, IOutputExpressionEmitter outputExpressionEmitter)
		{
			_dataScopeContract = dataScopeContract;
			_helperBinder = helperBinder;
			_dataBinder = dataBinder;
			_dataContextParameter = dataContextParameter;
			_outputExpressionEmitter = outputExpressionEmitter;
		}

		public override Expression Visit(Document document)
		{
			var expressions = document.ChildNodes.Select(node => node.Accept(this)).ToList();
			return _outputExpressionEmitter.HandleDocument(expressions);
		}

		public override Expression Visit(Element element)
		{
			return _outputExpressionEmitter.HandleElement(element, this);
		}

		public override Expression Visit(AttributeNode attributeNode)
		{
			var valueEmitter = attributeNode.Value.Accept(this);
			var expressions = _outputExpressionEmitter.HandleAttributeNode(attributeNode, valueEmitter);

			return Expression.Block(expressions);
		}

		public override Expression Visit(ConstantAttributeContent attributeContent)
		{
			return _outputExpressionEmitter.HandleAttributeContent(attributeContent);
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
			return _outputExpressionEmitter.HandleCompositeAttribute(compositeAttributeContent, this);
		}

		public override Expression Visit(MemberExpression memberExpression)
		{
			var scope = ScopeEmitter.Bind(_dataScopeContract, memberExpression);
			var binding = scope.RequiresString();

			var expression = binding.CreateExpression(_dataContextParameter);
			return _outputExpressionEmitter.HandleCall(expression);
		}

		public override Expression Visit(TextNode textNode)
		{
			return _outputExpressionEmitter.HandleTextNode(textNode);
		}

		private Expression HandleStatement(MustacheExpression expression, IEnumerable<HtmlNode> childNodes)
		{
			var iterationExpression = expression as IterationExpression;
			if (iterationExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, iterationExpression.Expression);

				IDataScopeContract childScopeContract;
				var binding = scope.RequiresEnumerable(out childScopeContract);

				var child = CreateVisitor(childScopeContract);
				var children = Many(childNodes.Select(c => c.Accept(child)).ToList());

				var collection = binding.CreateExpression(_dataContextParameter);
				return ExpressionHelper.ForEach(collection, child._dataContextParameter, children);
			}

			var conditionalExpression = expression as ConditionalExpression;
			if (conditionalExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, conditionalExpression.Expression);
				var binding = scope.RequiresBoolean();

				var children = Many(childNodes.Select(c => c.Accept(this)).ToList());

				var testExpression = binding.CreateExpression(_dataContextParameter);

				if (children.Type == typeof (void))
					return Expression.IfThen(testExpression, children);

				var returnTarget = Expression.Label(children.Type);

				var ex = Expression.IfThen(testExpression, Expression.Return(returnTarget, children));
				return Expression.Block(ex, Expression.Label(returnTarget, Expression.Constant(null, children.Type)));
			}

			var callHelperExpression = expression as CallHelperExpression;
			if (callHelperExpression != null)
			{
				var result = _helperBinder.FindByName(callHelperExpression.Name,
					CreateDictionaryFromArguments(callHelperExpression.Attributes));
				if (result == null)
					throw new Exception($"Unknown helper with name {callHelperExpression.Name}.");

				var children = Many(childNodes.Select(c => c.Accept(this)).ToList());

				var evaluation = result.CreateEmitter(new HelperBinderResult.HelperParameters(_outputExpressionEmitter, _helperBinder, _dataBinder, _dataScopeContract, _dataContextParameter), children);
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

		private static IDictionary<string, string> CreateDictionaryFromArguments(IEnumerable<HelperAttribute> attributes)
		{
			return attributes.ToDictionary(d => d.Name, d => d.Value);
		}

		private EmitExpressionVisitor CreateVisitor(IDataScopeContract childScopeContract)
		{
			var dataContextParameter = Expression.Parameter(childScopeContract.ResultType);
			return new EmitExpressionVisitor(childScopeContract, _helperBinder, _dataBinder, dataContextParameter,
				_outputExpressionEmitter);
		}
	}
}