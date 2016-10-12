using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class IncrementalDomOutputEmitter : IOutputExpressionEmitter
	{
		private Func<Expression, Expression> _handleExpression;
		private readonly IncrementalDomRendererExpressionBuilder _expressionBuilder;

		public IncrementalDomOutputEmitter(Expression renderer)
		{
			_expressionBuilder = new IncrementalDomRendererExpressionBuilder(renderer);
			_handleExpression = HandleElementCall;
		}

		public Expression HandleAttributeContent(ConstantAttributeContent attributeContent)
		{
			return Expression.Constant(attributeContent.Text, typeof(string));
		}

		public Expression HandleElement(Element element, INodeVisitor<Expression> visitor)
		{
			_handleExpression = HandleAttributeCall;

			var expressions = new List<Expression>();
			var staticAttributeNodes = element.Attributes.OfType<AttributeNode>().Where(e => e.Value is ConstantAttributeContent).ToList();
			var staticAttributeList = CreateAttributeDictionary(visitor, staticAttributeNodes);
			var attributeList = CreateAttributeDictionary(visitor, element.Attributes.Except(staticAttributeNodes));

			_handleExpression = HandleElementCall;

			if (element.ChildNodes.Count > 0)
			{
				expressions.Add(_expressionBuilder.ElementOpen(element.TagName, staticAttributeList, attributeList));
				expressions.AddRange(element.ChildNodes.Select(i => i.Accept(visitor)));
				expressions.Add(_expressionBuilder.ElementClose(element.TagName));

				return Expression.Block(expressions);
			}

			return _expressionBuilder.ElementVoid(element.TagName, staticAttributeList, attributeList);
		}

		private static Expression CreateAttributeDictionary(INodeVisitor<Expression> visitor, IEnumerable<ElementPart> attributes)
		{
			var type = typeof(Dictionary<string, string>);
			var addMethod = type.GetTypeInfo().GetMethod("Add");
			var nullDictionaryExpression = Expression.Constant(null, type);

			var attrExpressions = attributes.Select(e =>
			{
				var ex = e.Accept(visitor);
				return Expression.ElementInit(addMethod, Expression.Property(ex, "Key"), Expression.Property(ex, "Value"));
			}).ToList();

			return attrExpressions.Count > 0 ? (Expression) Expression.ListInit(Expression.New(type), attrExpressions) : nullDictionaryExpression;
		}

		public Expression HandleElementList(IReadOnlyList<Expression> elements)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<Expression> HandleAttributeNode(AttributeNode attributeNode, Expression valueEmitter)
		{
			var constructor = typeof(KeyValuePair<string, string>).GetTypeInfo().GetConstructor(new[] { typeof(string), typeof(string) });
			yield return Expression.New(constructor, Expression.Constant(attributeNode.Name), valueEmitter);
		}

		public Expression HandleCall(Expression callExpression)
		{
			return _handleExpression(callExpression);
		}

		private Expression HandleElementCall(Expression callExpression)
		{
			return _expressionBuilder.Text(callExpression);
		}

		private static Expression HandleAttributeCall(Expression callExpression)
		{
			return callExpression;
		}

		public Expression HandleTextNode(TextNode textNode)
		{
			return _expressionBuilder.Text(Expression.Constant(textNode.Text));
		}

		public Expression HandleDocument(List<Expression> expressions)
		{
			return Expression.Block(expressions);
		}

		public Expression HandleCompositeAttribute(CompositeAttributeContent compositeAttributeContent, INodeVisitor<Expression> visitor)
		{
			throw new NotImplementedException();
		}
	}
}