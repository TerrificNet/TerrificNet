using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	class IlExpressionEmitNodeVisitor : INodeVisitor
	{
		private readonly Stack<Scope> _scopes = new Stack<Scope>();

		public IlExpressionEmitNodeVisitor(IDataBinder dataBinder, IHelperBinder helperBinder)
		{

		}

		public Action<TextWriter, IDataContext, IRenderingContext> Generate()
		{
			var scope = _scopes.Pop();
			if (_scopes.Count != 0)
				throw new Exception("Scopes not closed");

			Action<TextWriter> exp = writer =>
			{
				foreach (var action in scope.Elements)
				{
					action(writer);
				}
			};

			return (writer, dataContext, renderingContext) => exp(writer);
		}

		public bool BeforeVisit(Document document)
		{
			_scopes.Push(new Scope());
			return true;
		}

		public void AfterVisit(Document document)
		{

		}

		public bool BeforeVisit(Element element)
		{
			_scopes.Push(new Scope());
			return true;
		}

		public void AfterVisit(Element element)
		{
			var scope = _scopes.Pop();
			Action<TextWriter> exp = writer =>
			{
				writer.Write($"<{element.TagName}");
				foreach (var attribute in scope.Attributes)
				{
					writer.Write(" ");
					attribute.Item1(writer);
					writer.Write("=\"");
					attribute.Item2(writer);
					writer.Write("\"");
				}
				writer.Write(">");

				foreach (var action in scope.Elements)
				{
					action(writer);
				}
				writer.Write($"</{element.TagName}>");
			};

			var parentScope = _scopes.Peek();
			parentScope.Elements.Add(exp);
		}

		public void Visit(ConstantAttributeContent attributeContent)
		{
			var scope = _scopes.Peek();
			scope.Elements.Add(writer => writer.Write(attributeContent.Text));
		}

		public bool BeforeVisit(Statement statement)
		{
			return true;
		}

		public void AfterVisit(Statement statement)
		{
			throw new NotImplementedException();
		}

		public void Visit(TextNode textNode)
		{
			Action<TextWriter> exp = writer => writer.Write(textNode.Text);
			_scopes.Peek().Elements.Add(exp);
		}

		public void Visit(AttributeContentStatement constantAttributeContent)
		{
			throw new NotImplementedException();
		}

		public bool BeforeVisit(AttributeNode attributeNode)
		{
			_scopes.Push(new Scope());
			return true;
		}

		public void AfterVisit(AttributeNode attributeNode)
		{
			var scope = _scopes.Pop();
			Action<TextWriter> a = w =>
			{
				foreach (var element in scope.Elements)
				{
					element(w);
				}
			};

			var parentScope = _scopes.Peek();
			parentScope.Attributes.Add(new Tuple<Action<TextWriter>, Action<TextWriter>>(w => w.Write(attributeNode.Name), a));
		}

		public bool BeforeVisit(CompositeAttributeContent compositeAttributeContent)
		{
			throw new NotImplementedException();
		}

		public void AfterVisit(CompositeAttributeContent compositeAttributeContent)
		{
			throw new NotImplementedException();
		}

		public bool BeforeVisit(AttributeStatement attributeStatement)
		{
			throw new NotImplementedException();
		}

		public void AfterVisit(AttributeStatement attributeStatement)
		{
			throw new NotImplementedException();
		}

		private class Scope
		{
			public List<Action<TextWriter>> Elements { get; }
			public List<Tuple<Action<TextWriter>, Action<TextWriter>>> Attributes { get; }

			public Scope()
			{
				Elements = new List<Action<TextWriter>>();
				Attributes = new List<Tuple<Action<TextWriter>, Action<TextWriter>>>();
			}
		}
	}
}