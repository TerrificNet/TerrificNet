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
		private readonly IDataBinder _dataBinder;
		private readonly Stack<Scope> _scopes = new Stack<Scope>();

		public IlExpressionEmitNodeVisitor(IDataBinder dataBinder, IHelperBinder helperBinder)
		{
			_dataBinder = dataBinder;
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
			Action<TextWriter, IDataContext, IRenderingContext> exp = (writer, dataContext, renderingContext) =>
			{
				writer.Write($"<{element.TagName}");
				foreach (var attribute in scope.Attributes)
				{
					writer.Write(" ");
					attribute.Item1(writer, dataContext, renderingContext);
					writer.Write("=\"");
					attribute.Item2(writer, dataContext, renderingContext);
					writer.Write("\"");
				}
				writer.Write(">");

				foreach (var action in scope.Elements)
				{
					action(writer, dataContext, renderingContext);
				}
				writer.Write($"</{element.TagName}>");
			};

			var parentScope = _scopes.Peek();
			parentScope.Elements.Add(exp);
		}

		public void Visit(ConstantAttributeContent attributeContent)
		{
			var scope = _scopes.Peek();
			scope.Elements.Add((writer, d, rc) => writer.Write(attributeContent.Text));
		}

		public bool BeforeVisit(Statement statement)
		{
			var expression = statement.Expression;

			var iterationExpression = expression as IterationExpression;
			if (iterationExpression != null)
			{
				var scope = new Scope();
				_scopes.Push(scope);

				var visitor = new EmitExpressionVisitor(_dataBinder, new NullHelperBinder());
				iterationExpression.Expression.Accept(visitor);

				var context = _dataBinder.Context();
				IEvaluator<IEnumerable> evaluator;
				if (!context.TryCreateEvaluation(out evaluator))
					return false;

				Action<TextWriter, IDataContext, IRenderingContext> ext = (writer, dataContext, renderingContext) =>
				{
					var items = evaluator.Evaluate(dataContext);
					foreach (var item in items)
					{

					}
				};
				scope.Elements.Add(ext);

				//IEvaluator<string> evaluator;
				//if (result.TryCreateEvaluation(out evaluator))
				//{
				//	scope.Elements.Add((writer, dataContext, renderingContext) =>
				//	{
				//		var value = evaluator.Evaluate(dataContext);
				//		writer.Write(value);
				//	});
				//}
			}

			return true;
		}

		public void AfterVisit(Statement statement)
		{
			var scope = _scopes.Peek();
			var expression = statement.Expression;

			var iterationExpression = expression as IterationExpression;
			if (iterationExpression != null)
			{
				throw new NotImplementedException();
			}

			var conditionalExpression = expression as ConditionalExpression;
			if (conditionalExpression != null)
			{
				throw new NotImplementedException();
			}

			var callHelperExpression = expression as CallHelperExpression;
			if (callHelperExpression != null)
			{
				throw new NotImplementedException();
			}

			var memberExpression = expression as MemberExpression;
			if (memberExpression != null)
			{
				var result = _dataBinder.Property(memberExpression.Name);
				IEvaluator<string> evaluator;
				if (result.TryCreateEvaluation(out evaluator))
				{
					scope.Elements.Add((writer, dataContext, renderingContext) =>
					{
						var value = evaluator.Evaluate(dataContext);
						writer.Write(value);
					});
				}
				return;
			}

			throw new NotImplementedException();
		}

		public void Visit(TextNode textNode)
		{
			Action<TextWriter, IDataContext, IRenderingContext> exp = (writer, d, rc) => writer.Write(textNode.Text);
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
			Action<TextWriter, IDataContext, IRenderingContext> a = (w, d, rc) =>
			{
				foreach (var element in scope.Elements)
				{
					element(w, d, rc);
				}
			};

			var parentScope = _scopes.Peek();
			parentScope.Attributes.Add(new Tuple<Action<TextWriter, IDataContext, IRenderingContext>, Action<TextWriter, IDataContext, IRenderingContext>>((w, d, r) => w.Write(attributeNode.Name), a));
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
			public List<Action<TextWriter, IDataContext, IRenderingContext>> Elements { get; }
			public List<Tuple<Action<TextWriter, IDataContext, IRenderingContext>, Action<TextWriter, IDataContext, IRenderingContext>>> Attributes { get; }

			public Scope()
			{
				Elements = new List<Action<TextWriter, IDataContext, IRenderingContext>>();
				Attributes = new List<Tuple<Action<TextWriter, IDataContext, IRenderingContext>, Action<TextWriter, IDataContext, IRenderingContext>>>();
			}
		}
	}
}