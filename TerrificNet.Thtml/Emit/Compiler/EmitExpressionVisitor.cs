using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
		private readonly ParameterExpression _writerParameter = Expression.Parameter(typeof(TextWriter));
		private readonly ParameterExpression _dataContextParameter;

		public Action<TextWriter, object> DocumentFunc { get; private set; }

		public EmitExpressionVisitor(IDataScopeContract dataScopeContract, IHelperBinder<Expression, ExpressionHelperConfig> helperBinder, ParameterExpression writerParameter = null)
		{
			_dataScopeContract = dataScopeContract;
			_helperBinder = helperBinder;
			_dataContextParameter = Expression.Variable(_dataScopeContract.ResultType, "item");

			if (writerParameter != null)
				_writerParameter = writerParameter;
		}

		public override Expression Visit(Document document)
		{
			var expression = Many(document.ChildNodes.Select(node => node.Accept(this)).ToList());

			var inputExpression = Expression.Parameter(typeof(object), "input");
			var convertExpression = Expression.Assign(_dataContextParameter, Expression.ConvertChecked(inputExpression, _dataScopeContract.ResultType));
			var bodyExpression = Expression.Block(new[] { _dataContextParameter }, convertExpression, expression);
			DocumentFunc = Expression.Lambda<Action<TextWriter, object>>(bodyExpression, _writerParameter, inputExpression).Compile();

			return expression;
		}

		public override Expression Visit(Element element)
		{
			var expressions = new List<Expression>();
			expressions.Add(Write($"<{element.TagName}"));
			expressions.AddRange(element.Attributes.Select(attribute => attribute.Accept(this)));
			expressions.Add(Write(">"));
			expressions.AddRange(element.ChildNodes.Select(i => i.Accept(this)));
			expressions.Add(Write($"</{element.TagName}>"));

			return Expression.Block(expressions);
		}

		public override Expression Visit(AttributeNode attributeNode)
		{
			var valueEmitter = attributeNode.Value.Accept(this);

			return Expression.Block(Write(" " + attributeNode.Name + "=\""),
				valueEmitter,
				Write("\""));
		}

		public override Expression Visit(ConstantAttributeContent attributeContent)
		{
			return Write(attributeContent.Text);
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

			var evaluator = scope.RequiresString();
			var evaluateMethod = ExpressionHelper.GetMethodInfo<IEvaluator<string>>(i => i.Evaluate(null));
			var callExpression = Expression.Call(Expression.Constant(evaluator), evaluateMethod, _dataContextParameter);
			return Write(callExpression);
		}

		public override Expression Visit(TextNode textNode)
		{
			return Write(textNode.Text);
		}

		private Expression HandleStatement(MustacheExpression expression, IEnumerable<HtmlNode> childNodes)
		{
			var iterationExpression = expression as IterationExpression;
			if (iterationExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, iterationExpression.Expression);

				IDataScopeContract childScopeContract;
				var evaluator = scope.RequiresEnumerable(out childScopeContract);

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
				var evaluator = scope.RequiresBoolean();

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

				var config = new ExpressionHelperConfig { WriterParameter = _writerParameter };
				var evaluation = result.CreateEmitter(config, children, _helperBinder, _dataScopeContract);
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
			return new EmitExpressionVisitor(childScopeContract, _helperBinder, _writerParameter);
		}

		private Expression Write(Expression inputExpression)
		{
			return ExpressionHelper.Write(_writerParameter, inputExpression);
		}

		private Expression Write(string value)
		{
			return ExpressionHelper.Write(_writerParameter, value);
		}
	}
}