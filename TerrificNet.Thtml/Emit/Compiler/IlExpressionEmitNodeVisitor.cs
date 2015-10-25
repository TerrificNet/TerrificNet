using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Emit.Compiler
{
	class IlExpressionEmitNodeVisitor : INodeVisitor
	{
		private readonly EmitExpressionVisitor _expressionVisitor;
		private readonly Stack<ScopeItem> _scopes = new Stack<ScopeItem>();

		private ScopeItem Scope => _scopes.Peek();

		public IlExpressionEmitNodeVisitor(IDataBinder dataBinder, IHelperBinder helperBinder)
		{
			_expressionVisitor = new EmitExpressionVisitor(dataBinder, helperBinder);
		}

		public Action<TextWriter, IDataContext, IRenderingContext> Generate()
		{
			var scope = _scopes.Pop();
			if (_scopes.Count != 0)
				throw new Exception("Scopes not closed");

			Action<TextWriter, IDataContext, IRenderingContext> exp = (writer, dataContext, renderingContext) =>
			{
				foreach (var action in scope.Elements)
				{
					action(writer, dataContext, renderingContext);
				}
			};

			return exp;
		}

		public void Visit(Element element)
		{
			var scope = _scopes.Peek();

			scope.Elements.Add((writer, dataContext, renderingContext) => writer.Write($"<{element.TagName}"));
			foreach (var attribute in element.Attributes)
			{
				attribute.Accept(this);
			}
			scope.Elements.Add((writer, dataContext, renderingContext) => writer.Write(">"));

			foreach (var node in element.ChildNodes)
			{
				node.Accept(this);
			}

			scope.Elements.Add((writer, dataContext, renderingContext) => writer.Write($"</{element.TagName}>"));
		}

		public void Visit(ConstantAttributeContent attributeContent)
		{
			var scope = _scopes.Peek();
			scope.Elements.Add((writer, d, rc) => writer.Write(attributeContent.Text));
		}

		public void Visit(TextNode textNode)
		{
			_scopes.Peek().Elements.Add((writer, d, rc) => writer.Write(textNode.Text));
		}

		public void Visit(AttributeNode attributeNode)
		{
			var scope = _scopes.Peek();
			scope.Elements.Add((writer, dataContext, renderingContext) => writer.Write($" {attributeNode.Name}=\""));
			attributeNode.Value.Accept(this);
			scope.Elements.Add((writer, dataContext, renderingContext) => writer.Write("\""));
		}

		public void Visit(AttributeContentStatement constantAttributeContent)
		{
			throw new NotImplementedException();
		}

		public void Visit(Document document)
		{
			_scopes.Push(new ScopeItem());

			foreach (var node in document.ChildNodes)
			{
				node.Accept(this);
			}
		}

		public void Visit(CallHelperExpression callHelperExpression)
		{
			throw new NotImplementedException();
		}

		public void Visit(UnconvertedExpression unconvertedExpression)
		{
			throw new NotImplementedException();
		}

		public void Visit(IterationExpression iterationExpression)
		{
			throw new NotImplementedException();
		}

		public void Visit(ConditionalExpression conditionalExpression)
		{
			throw new NotImplementedException();
		}

		public void Visit(MemberExpression memberExpression)
		{
			throw new NotImplementedException();
		}

		public void Visit(Statement statement)
		{
			statement.Expression.Accept(this);
			_scopes.Push(new ScopeItem());

			foreach (var childNode in statement.ChildNodes)
			{
				childNode.Accept(this);
			}
			
		}

		private class ScopeItem
		{
			public List<Action<TextWriter, IDataContext, IRenderingContext>> Elements { get; }

			public ScopeItem()
			{
				Elements = new List<Action<TextWriter, IDataContext, IRenderingContext>>();
			}
		}
	}
}