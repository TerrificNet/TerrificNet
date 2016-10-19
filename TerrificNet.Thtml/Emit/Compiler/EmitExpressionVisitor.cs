using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TerrificNet.Thtml.Formatting;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using ConditionalExpression = TerrificNet.Thtml.Parsing.Handlebars.ConditionalExpression;
using MemberExpression = TerrificNet.Thtml.Parsing.Handlebars.MemberExpression;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class EmitExpressionVisitor : NodeVisitorBase<Expression>, INodeCompilerVisitor
	{
		private readonly IDataScopeContract _dataScopeContract;
		private readonly IHelperBinder _helperBinder;
		private readonly CompilerExtensions _extensions;
		private readonly Expression _renderingContextExpression;
		private readonly IOutputExpressionBuilder _expressionBuilder;

		public EmitExpressionVisitor(IDataScopeContract dataScopeContract, CompilerExtensions extensions, Expression renderingContextExpression)
		{
			_dataScopeContract = dataScopeContract;
			_extensions = extensions;
			_renderingContextExpression = renderingContextExpression;
			_helperBinder = _extensions.HelperBinder;
			_expressionBuilder = _extensions.ExpressionBuilder;
		}

		public override Expression Visit(Document document)
		{
			var expressions = document.ChildNodes.Select(node => node.Accept(this)).ToList();
			return Expression.Block(expressions);
		}

		public override Expression Visit(Element element)
		{
			var tagResult = _extensions.TagHelper.FindByName(element);
			if (tagResult != null)
			{
				return tagResult.CreateExpression(new HelperParameters(_dataScopeContract, this, _extensions, _renderingContextExpression));
			}

			var expressions = new List<Expression>();
			var staticAttributeNodes = element.Attributes.Where(e => e.IsFixed).ToList();
			var staticAttributeList = CreateAttributeDictionary(staticAttributeNodes);
			var attributeList = element.Attributes.Except(staticAttributeNodes).ToList();

			if (attributeList.Count > 0)
			{
				expressions.Add(_expressionBuilder.ElementOpenStart(element.TagName, staticAttributeList));
				expressions.AddRange(element.Attributes.Select(attribute => attribute.Accept(this)));
				expressions.Add(_expressionBuilder.ElementOpenEnd());
			}
			else
				expressions.Add(_expressionBuilder.ElementOpen(element.TagName, staticAttributeList));

			expressions.AddRange(element.ChildNodes.Select(i => i.Accept(this)));

			expressions.Add(_expressionBuilder.ElementClose(element.TagName));

			return Expression.Block(expressions);
		}

		public override Expression Visit(AttributeNode attributeNode)
		{
			var expressions = new List<Expression>
			{
				_expressionBuilder.PropertyStart(attributeNode.Name),
				attributeNode.Value.Accept(this),
				_expressionBuilder.PropertyEnd()
			};

			return Expression.Block(expressions);
		}

		public override Expression Visit(ConstantAttributeContent attributeContent)
		{
			return _expressionBuilder.Value(Expression.Constant(attributeContent.Text));
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
			return Expression.Block(compositeAttributeContent.ContentParts.Select(p => p.Accept(this)).ToList());
		}

		public override Expression Visit(MemberExpression memberExpression)
		{
			return HandleCall(memberExpression);
		}

		public override Expression Visit(ParentExpression parentExpression)
		{
			return HandleCall(parentExpression);
		}

		public override Expression Visit(SelfExpression selfExpression)
		{
			return HandleCall(selfExpression);
		}

		private Expression HandleCall(MustacheExpression memberExpression)
		{
			var scope = ScopeEmitter.Bind(_dataScopeContract, memberExpression);
			var binding = scope.RequiresString();

			var expression = binding.Expression;
			return _expressionBuilder.Value(expression);
		}

		public override Expression Visit(TextNode textNode)
		{
			return _expressionBuilder.Value(Expression.Constant(textNode.Text));
		}

		private Expression HandleStatement(MustacheExpression expression, IEnumerable<HtmlNode> childNodes)
		{
			var iterationExpression = expression as IterationExpression;
			if (iterationExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, iterationExpression.Expression);

				IDataScopeContract childScopeContract;
				var binding = scope.RequiresEnumerable(out childScopeContract);

				var child = ChangeContract(childScopeContract);
				var children = childNodes.Select(c => c.Accept(child)).ToList();

				var collection = binding.Expression;
				return ExpressionHelper.ForEach(collection, (ParameterExpression) childScopeContract.Expression, children);
			}

			var conditionalExpression = expression as ConditionalExpression;
			if (conditionalExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, conditionalExpression.Expression);
				var binding = scope.RequiresBoolean();

				var children = Many(childNodes.Select(c => c.Accept(this)).ToList());

				var testExpression = binding.Expression;

				return Expression.IfThen(testExpression, children);
			}

			var callHelperExpression = expression as CallHelperExpression;
			if (callHelperExpression != null)
			{
				var result = _helperBinder.FindByName(callHelperExpression.Name, CreateDictionaryFromArguments(callHelperExpression.Attributes));
				if (result == null)
					throw new Exception($"Unknown helper with name {callHelperExpression.Name}.");

				return result.CreateExpression(new HelperParameters(_dataScopeContract, this, _extensions, _renderingContextExpression));
			}

			var contentEmitter = expression.Accept(this);
			if (contentEmitter != null)
				return contentEmitter;

			return Many(childNodes.Select(childNode => childNode.Accept(this)).ToList());
		}

		private static Expression Many(IReadOnlyCollection<Expression> expressions)
		{
			return expressions.Count > 0 ? (Expression) Expression.Block(expressions) : Expression.Empty();
		}

		private static IDictionary<string, string> CreateDictionaryFromArguments(IEnumerable<HelperAttribute> attributes)
		{
			return attributes.ToDictionary(d => d.Name, d => d.Value);
		}

		public INodeCompilerVisitor ChangeContract(IDataScopeContract childScopeContract)
		{
			return new EmitExpressionVisitor(childScopeContract, _extensions, _renderingContextExpression);
		}

		public INodeCompilerVisitor ChangeExtensions(CompilerExtensions extensions)
		{
			return new EmitExpressionVisitor(_dataScopeContract, extensions, _renderingContextExpression);
		}

		public Expression Visit(IEnumerable<Node> nodes)
		{
			return Expression.Block(nodes.Select(i => i.Accept(this)));
		}

		private static IReadOnlyDictionary<string, string> CreateAttributeDictionary(IEnumerable<ElementPart> staticAttributeNodes)
		{
			var dict = new Dictionary<string, string>();

			var visitor = new AttributeDictionaryVisitor(dict);
			foreach (var node in staticAttributeNodes)
			{
				node.Accept(visitor);
			}

			return dict;
		}
	}
}