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
		private readonly IExpressionBuilder _exBuilder;

		public EmitExpressionVisitor(IDataScopeContract dataScopeContract, CompilerExtensions extensions, Expression renderingContextExpression, IExpressionBuilder expressionBuilder)
		{
			_dataScopeContract = dataScopeContract;
			_extensions = extensions;
			_renderingContextExpression = renderingContextExpression;
			_helperBinder = _extensions.HelperBinder;
			_expressionBuilder = _extensions.ExpressionBuilder;
			_exBuilder = expressionBuilder;
		}

		public override Expression Visit(Document document)
		{
			foreach (var child in document.ChildNodes)
				child.Accept(this);

			return null;
		}

		public override Expression Visit(Element element)
		{
			var tagResult = _extensions.TagHelper.FindByName(element);
			if (tagResult != null)
			{
				tagResult.CreateExpression(new HelperParameters(_dataScopeContract, this, _extensions, _renderingContextExpression));
				return null;
			}

			var staticAttributeNodes = element.Attributes.Where(e => e.IsFixed).ToList();
			var staticAttributeList = CreateAttributeDictionary(staticAttributeNodes);
			var attributeList = element.Attributes.Except(staticAttributeNodes).ToList();

			if (attributeList.Count > 0)
			{
				_exBuilder.Add(_expressionBuilder.ElementOpenStart(element.TagName, staticAttributeList));
				foreach (var attr in element.Attributes)
					attr.Accept(this);

				_exBuilder.Add(_expressionBuilder.ElementOpenEnd());
			}
			else
				_exBuilder.Add(_expressionBuilder.ElementOpen(element.TagName, staticAttributeList));

			foreach (var child in element.ChildNodes)
				child.Accept(this);

			_exBuilder.Add(_expressionBuilder.ElementClose(element.TagName));

			return null;
		}

		public override Expression Visit(AttributeNode attributeNode)
		{
			_exBuilder.Add(_expressionBuilder.PropertyStart(attributeNode.Name));
			attributeNode.Value.Accept(this);
			_exBuilder.Add(_expressionBuilder.PropertyEnd());

			return null;
		}

		public override Expression Visit(ConstantAttributeContent attributeContent)
		{
			_exBuilder.Add(_expressionBuilder.Value(Expression.Constant(attributeContent.Text)));
			return null;
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
			_exBuilder.Add(unconvertedExpression.Expression.Accept(this));
			return null;
		}

		public override Expression Visit(CompositeAttributeContent compositeAttributeContent)
		{
			foreach (var part in compositeAttributeContent.ContentParts)
				part.Accept(this);

			return null;
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
			_exBuilder.Add(_expressionBuilder.Value(expression));

			return null;
		}

		public override Expression Visit(TextNode textNode)
		{
			_exBuilder.Add(_expressionBuilder.Value(Expression.Constant(textNode.Text)));
			return null;
		}

		private Expression HandleStatement(MustacheExpression expression, IEnumerable<HtmlNode> childNodes)
		{
			var iterationExpression = expression as IterationExpression;
			if (iterationExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, iterationExpression.Expression);

				IDataScopeContract childScopeContract;
				var binding = scope.RequiresEnumerable(out childScopeContract);

				Action<Expression> childrenAction = l =>
				{
					var child = ChangeContract(childScopeContract);
					foreach (var child2 in childNodes)
						child2.Accept(child);
				};

				var collection = binding.Expression;

				_exBuilder.Foreach(collection, childrenAction, (ParameterExpression) childScopeContract.Expression);

				return null;
			}

			var conditionalExpression = expression as ConditionalExpression;
			if (conditionalExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, conditionalExpression.Expression);
				var binding = scope.RequiresBoolean();
				var testExpression = binding.Expression;

				Action children = () =>
				{
					foreach (var c in childNodes)
						c.Accept(this);
				};

				_exBuilder.IfThen(testExpression, children);
				return null;
			}

			var callHelperExpression = expression as CallHelperExpression;
			if (callHelperExpression != null)
			{
				var result = _helperBinder.FindByName(callHelperExpression.Name, CreateDictionaryFromArguments(callHelperExpression.Attributes));
				if (result == null)
					throw new Exception($"Unknown helper with name {callHelperExpression.Name}.");

				result.CreateExpression(new HelperParameters(_dataScopeContract, this, _extensions, _renderingContextExpression));
				return null;
			}

			var contentEmitter = expression.Accept(this);
			if (contentEmitter != null)
				return contentEmitter;

			foreach (var child in childNodes)
				child.Accept(this);

			return null;
		}

		private static IDictionary<string, string> CreateDictionaryFromArguments(IEnumerable<HelperAttribute> attributes)
		{
			return attributes.ToDictionary(d => d.Name, d => d.Value);
		}

		public INodeCompilerVisitor ChangeContract(IDataScopeContract childScopeContract)
		{
			return new EmitExpressionVisitor(childScopeContract, _extensions, _renderingContextExpression, _exBuilder);
		}

		public INodeCompilerVisitor ChangeExtensions(CompilerExtensions extensions)
		{
			return new EmitExpressionVisitor(_dataScopeContract, extensions, _renderingContextExpression, _exBuilder);
		}

		public Expression Visit(IEnumerable<Node> nodes)
		{
			foreach (var node in nodes)
				node.Accept(this);

			return null;
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