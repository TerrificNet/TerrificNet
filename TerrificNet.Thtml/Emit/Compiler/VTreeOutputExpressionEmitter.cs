using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class VTreeOutputExpressionEmitter
	{
		private readonly IOutputExpressionBuilder _builder;

		internal VTreeOutputExpressionEmitter(IOutputExpressionBuilder expressionBuilder)
		{
			_builder = expressionBuilder;
		}

		public Expression HandleAttributeContent(ConstantAttributeContent attributeContent)
		{
			return _builder.Value(Expression.Constant(attributeContent.Text));
		}

		public Expression HandleElement(Element element, INodeVisitor<Expression> visitor)
		{
			var expressions = new List<Expression>();
			var staticAttributeNodes = element.Attributes.Where(e => e.IsFixed).ToList();
			var staticAttributeList = CreateAttributeDictionary(staticAttributeNodes);
			var attributeList = element.Attributes.Except(staticAttributeNodes).ToList();

			if (attributeList.Count > 0)
			{
				expressions.Add(_builder.ElementOpenStart(element.TagName, staticAttributeList));
				expressions.AddRange(element.Attributes.Select(attribute => attribute.Accept(visitor)));
				expressions.Add(_builder.ElementOpenEnd());
			}
			else
				expressions.Add(_builder.ElementOpen(element.TagName, staticAttributeList));

			expressions.AddRange(element.ChildNodes.Select(i => i.Accept(visitor)));

			expressions.Add(_builder.ElementClose(element.TagName));

			return Expression.Block(expressions);
		}

		private static IReadOnlyDictionary<string, string> CreateAttributeDictionary(List<ElementPart> staticAttributeNodes)
		{
			var dict = new Dictionary<string, string>();

			var visitor = new AttributeDictionaryVisitor(dict);
			foreach (var node in staticAttributeNodes)
			{
				node.Accept(visitor);
			}

			return dict;
		}

		public Expression HandleElementList(IReadOnlyList<Expression> elements)
		{
			return Expression.Block(elements);
		}

		public IEnumerable<Expression> HandleAttributeNode(AttributeNode attributeNode, INodeVisitor<Expression> visitor)
		{
			yield return _builder.PropertyStart(attributeNode.Name);
			yield return attributeNode.Value.Accept(visitor);
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

		private class AttributeDictionaryVisitor : NodeVisitorBase<string>
		{
			private readonly IDictionary<string, string> _dict;

			public AttributeDictionaryVisitor(IDictionary<string, string> dict)
			{
				_dict = dict;
			}

			public override string Visit(AttributeNode attributeNode)
			{
				_dict.Add(attributeNode.Name, attributeNode.Value.Accept(this));

				return null;
			}

			public override string Visit(ConstantAttributeContent attributeContent)
			{
				return attributeContent.Text;
			}

			public override string Visit(CompositeAttributeContent compositeAttributeContent)
			{
				return compositeAttributeContent.ContentParts.Aggregate(new StringBuilder(), (sb, a) => sb.Append(a.Accept(this))).ToString();
			}
		}
	}
}