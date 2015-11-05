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
		private readonly ParameterExpression _dataContextParameter;
		private readonly IOutputExpressionEmitter _outputExpressionEmitter;

		public EmitExpressionVisitor(IDataScopeContract dataScopeContract, IHelperBinder<Expression, ExpressionHelperConfig> helperBinder, ParameterExpression dataContextParameter, IOutputExpressionEmitter outputExpressionEmitter)
		{
			_dataScopeContract = dataScopeContract;
			_helperBinder = helperBinder;
			_dataContextParameter = dataContextParameter;
			_outputExpressionEmitter = outputExpressionEmitter;
		}

		public override Expression Visit(Document document)
		{
			return Many(document.ChildNodes.Select(node => node.Accept(this)).ToList());
		}

		public override Expression Visit(Element element)
		{
			var expressions = _outputExpressionEmitter.HandleElement(element, this);

			return Expression.Block(expressions);
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
			return Many(compositeAttributeContent.ContentParts.Select(p => p.Accept(this)).ToList());
		}

		public override Expression Visit(MemberExpression memberExpression)
		{
			var scope = ScopeEmitter.Bind(_dataScopeContract, memberExpression);
			var binding = scope.RequiresString();

			var evaluator = binding.CreateEvaluator();
			var evaluateMethod = ExpressionHelper.GetMethodInfo<IEvaluator<string>>(i => i.Evaluate(null));
			var callExpression = Expression.Call(Expression.Constant(evaluator), evaluateMethod, _dataContextParameter);
			return _outputExpressionEmitter.HandleCall(callExpression);
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

				var evaluation = result.CreateEmitter(_outputExpressionEmitter, children, _helperBinder, _dataScopeContract);
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
			return new EmitExpressionVisitor(childScopeContract, _helperBinder, dataContextParameter, _outputExpressionEmitter);
		}	
	}
}