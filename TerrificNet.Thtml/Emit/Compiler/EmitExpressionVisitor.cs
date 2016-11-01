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
	internal class EmitExpressionVisitor : NodeVisitorBase, INodeCompilerVisitor
	{
		private readonly IDataScopeContract _dataScopeContract;
		private readonly IHelperBinder _helperBinder;
		private readonly CompilerExtensions _extensions;
		private readonly Expression _renderingContextExpression;
		private readonly IOutputExpressionBuilder _formatter;
		private readonly IScopedExpressionBuilder _exBuilder;

		public EmitExpressionVisitor(IDataScopeContract dataScopeContract, CompilerExtensions extensions, Expression renderingContextExpression, IScopedExpressionBuilder expressionBuilder)
		{
			_dataScopeContract = dataScopeContract;
			_extensions = extensions;
			_renderingContextExpression = renderingContextExpression;
			_helperBinder = _extensions.HelperBinder;
			_formatter = _extensions.ExpressionBuilder;
			_exBuilder = expressionBuilder;
		}

		private void ReportBinding(IBinding binding)
		{
			_exBuilder.UseBinding(binding);
		}

		public override void Visit(Document document)
		{
			HandleChildNodes(document);
		}

		private void HandleChildNodes(Document document)
		{
			if (document.ChildNodes.Count == 0)
				return;

			_exBuilder.Enter();

			foreach (var child in document.ChildNodes)
				child.Accept(this);

			_exBuilder.Leave();
		}

		public override void Visit(Element element)
		{
			_exBuilder.Enter();

			var tagResult = _extensions.TagHelper.FindByName(element);
			if (tagResult != null)
			{
				tagResult.Visit(new HelperParameters(_dataScopeContract, this, _extensions, _renderingContextExpression, _exBuilder));
				return;
			}

			var staticAttributeNodes = element.Attributes.Where(e => e.IsFixed).ToList();
			var staticAttributeList = CreateAttributeDictionary(staticAttributeNodes);
			var attributeList = element.Attributes.Except(staticAttributeNodes).ToList();

			if (attributeList.Count > 0)
			{
				_formatter.ElementOpenStart(_exBuilder, element.TagName, staticAttributeList);
				foreach (var attr in element.Attributes)
					attr.Accept(this);

				_formatter.ElementOpenEnd(_exBuilder);
			}
			else
				_formatter.ElementOpen(_exBuilder, element.TagName, staticAttributeList);

			HandleChildNodes(element);

			_formatter.ElementClose(_exBuilder, element.TagName);

			_exBuilder.Leave();
		}

		public override void Visit(AttributeNode attributeNode)
		{
			_exBuilder.Enter();

			_formatter.PropertyStart(_exBuilder, attributeNode.Name);
			attributeNode.Value.Accept(this);
			_formatter.PropertyEnd(_exBuilder);

			_exBuilder.Leave();
		}

		public override void Visit(ConstantAttributeContent attributeContent)
		{
			_formatter.Text(_exBuilder, attributeContent.Text);
		}

		public override void Visit(AttributeContentStatement constantAttributeContent)
		{
			HandleStatement(constantAttributeContent.Expression, constantAttributeContent.Children);
		}

		public override void Visit(Statement statement)
		{
			var expression = statement.Expression;
			HandleStatement(expression, statement.ChildNodes);
		}

		public override void Visit(UnconvertedExpression unconvertedExpression)
		{
			unconvertedExpression.Expression.Accept(this);
		}

		public override void Visit(CompositeAttributeContent compositeAttributeContent)
		{
			foreach (var part in compositeAttributeContent.ContentParts)
				part.Accept(this);
		}

		public override void Visit(MemberExpression memberExpression)
		{
			HandleCall(memberExpression);
		}

		public override void Visit(ParentExpression parentExpression)
		{
			HandleCall(parentExpression);
		}

		public override void Visit(SelfExpression selfExpression)
		{
			HandleCall(selfExpression);
		}

		private void HandleCall(MustacheExpression memberExpression)
		{
			var scope = ScopeEmitter.Bind(_dataScopeContract, memberExpression);
			var binding = scope.RequiresString();

			ReportBinding(binding);

			_formatter.Value(_exBuilder, binding);
		}

		public override void Visit(TextNode textNode)
		{
			_formatter.Text(_exBuilder, textNode.Text);
		}

		private void HandleStatement(MustacheExpression expression, IEnumerable<HtmlNode> childNodes)
		{
			var iterationExpression = expression as IterationExpression;
			if (iterationExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, iterationExpression.Expression);
				ReportBinding(scope);

				IDataScopeContract childScopeContract;
				var binding = scope.RequiresEnumerable(out childScopeContract).EnsureBinding();
				ReportBinding(binding);

				Action<Expression> childrenAction = l =>
				{
					var childVisitor = ChangeContract(childScopeContract);
					foreach (var child in childNodes)
						child.Accept(childVisitor);
				};

				var collection = binding.Expression;
				var childScopeBinding = childScopeContract.EnsureBinding();
				ReportBinding(childScopeBinding);

				_exBuilder.Foreach(collection, childrenAction, (ParameterExpression)childScopeBinding.Expression);

				return;
			}

			var conditionalExpression = expression as ConditionalExpression;
			if (conditionalExpression != null)
			{
				var scope = ScopeEmitter.Bind(_dataScopeContract, conditionalExpression.Expression);
				var binding = scope.RequiresBoolean().EnsureBinding();
				ReportBinding(binding);

				var testExpression = binding.Expression;

				Action children = () =>
				{
					foreach (var c in childNodes)
						c.Accept(this);
				};

				_exBuilder.IfThen(testExpression, children);
				return;
			}

			var callHelperExpression = expression as CallHelperExpression;
			if (callHelperExpression != null)
			{
				var result = _helperBinder.FindByName(callHelperExpression.Name, CreateDictionaryFromArguments(callHelperExpression.Attributes));
				if (result == null)
					throw new Exception($"Unknown helper with name {callHelperExpression.Name}.");

				result.Visit(new HelperParameters(_dataScopeContract, this, _extensions, _renderingContextExpression, _exBuilder));
				return;
			}

			expression.Accept(this);

			foreach (var child in childNodes)
				child.Accept(this);
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

		public void Visit(IEnumerable<Node> nodes)
		{
			foreach (var node in nodes)
				node.Accept(this);
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