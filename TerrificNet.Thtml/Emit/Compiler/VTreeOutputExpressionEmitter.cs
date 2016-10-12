using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class VTreeOutputExpressionEmitter : IOutputExpressionEmitter
	{
		private readonly VDomBuilderExpression _builder;

		public VTreeOutputExpressionEmitter(Expression builderParameter)
		{
			_builder = new VDomBuilderExpression(builderParameter);
		}

		public Expression HandleAttributeContent(ConstantAttributeContent attributeContent)
		{
			return _builder.Value(Expression.Constant(attributeContent.Text));
		}

		public Expression HandleElement(Element element, INodeVisitor<Expression> visitor)
		{
			var expressions = new List<Expression>();
			if (element.Attributes.Count > 0)
			{
				expressions.Add(_builder.ElementOpenStart(Expression.Constant(element.TagName)));
				expressions.AddRange(element.Attributes.Select(attribute => attribute.Accept(visitor)));
				expressions.Add(_builder.ElementOpenEnd());
			}
			else
				expressions.Add(_builder.ElementOpen(Expression.Constant(element.TagName)));

			expressions.AddRange(element.ChildNodes.Select(i => i.Accept(visitor)));

			expressions.Add(_builder.ElementClose());

			return Expression.Block(expressions);
		}

		public Expression HandleElementList(IReadOnlyList<Expression> elements)
		{
			return Expression.Block(elements);
		}

		public IEnumerable<Expression> HandleAttributeNode(AttributeNode attributeNode, Expression valueEmitter)
		{
			yield return _builder.PropertyStart(Expression.Constant(attributeNode.Name));
			yield return valueEmitter;
			yield return _builder.PropertyEnd();
		}

		public Expression HandleCall(Expression callExpression)
		{
			return _builder.Value(callExpression);
		}

		public Expression HandleTextNode(TextNode textNode)
		{
			return _builder.Value(Expression.Constant(textNode.Text));
		}

		public Expression HandleDocument(List<Expression> expressions)
		{
			return Expression.Block(expressions);
		}

		public Expression HandleCompositeAttribute(CompositeAttributeContent compositeAttributeContent, INodeVisitor<Expression> visitor)
		{
			return Expression.Block(compositeAttributeContent.ContentParts.Select(p => p.Accept(visitor)).ToList());
		}
	}
}