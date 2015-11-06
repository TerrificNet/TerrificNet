using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class VTreeOutputExpressionEmitter : IOutputExpressionEmitter
	{
		private Func<Expression, Expression> _handleExpression = HandleElementCall;
		private ParameterExpression _stringBuilder;

		public Expression HandleAttributeContent(ConstantAttributeContent attributeContent)
		{
			return _handleExpression(Expression.Constant(attributeContent.Text));
		}

		private static Expression GetAttributeValue(Expression textExpression)
		{
			var constructor = typeof(StringVPropertyValue).GetConstructor(new[] { typeof(string) });

			return Expression.New(constructor, textExpression);
		}

		public Expression HandleElement(Element element, INodeVisitor<Expression> visitor)
		{
			_handleExpression = HandleAttributeValueCall;
			var attributes = element.Attributes.Select(attribute => attribute.Accept(visitor)).ToList();

			_handleExpression = HandleElementCall;
			var elements = element.ChildNodes.Select(i => i.Accept(visitor)).ToList();

			var attributeList = attributes.Count > 0 ? (Expression)Expression.ListInit(Expression.New(typeof(List<VProperty>)), attributes) : Expression.Constant(null, typeof(IEnumerable<VProperty>));
			var elementList = CreateElementList(elements);

			var constructorInfo = typeof(VElement).GetConstructor(new[] { typeof(string), typeof(IEnumerable<VProperty>), typeof(IEnumerable<VTree>) });

			return Expression.New(constructorInfo, Expression.Constant(element.TagName), attributeList, elementList);
		}

		private static Expression CreateElementList(IReadOnlyCollection<Expression> elements)
		{
			if (elements.Count == 0)
				return Expression.Constant(null, typeof (IEnumerable<VTree>));

			if (elements.All(e => typeof(VTree).IsAssignableFrom(e.Type)))
				return Expression.ListInit(Expression.New(typeof (List<VTree>)), elements);

			var expressions = new List<Expression>();
			var variable = Expression.Variable(typeof (List<VTree>));
			var createAssign = Expression.Assign(variable, Expression.New(typeof (List<VTree>)));
			expressions.Add(createAssign);

			var method = typeof (List<VTree>).GetMethod("Add", new[] {typeof (VTree)});
			var methodRange = typeof(List<VTree>).GetMethod("AddRange", new[] { typeof(IEnumerable<VTree>) });

			foreach (var elem in elements)
			{
				if (typeof (VTree).IsAssignableFrom(elem.Type))
					expressions.Add(Expression.Call(variable, method, elem));
				else
					expressions.Add(Expression.Call(variable, methodRange, elem));
			}

			expressions.Add(variable);

			return Expression.Block(new [] { variable }, expressions);
		}

		public IEnumerable<Expression> HandleAttributeNode(AttributeNode attributeNode, Expression valueEmitter)
		{
			var constructor = typeof(VProperty).GetConstructor(new[] { typeof(string), typeof(VPropertyValue) });
			yield return Expression.New(constructor, Expression.Constant(attributeNode.Name), valueEmitter);
		}

		public Expression HandleCall(Expression callExpression)
		{
			return _handleExpression(callExpression);
		}

		private static Expression HandleElementCall(Expression callExpression)
		{
			return GetText(callExpression);
		}

		private Expression HandleCompositeAttributeValue(Expression callExpression)
		{
			var method = typeof (StringBuilder).GetMethod("Append", new[] {typeof (string)});
			return Expression.Call(_stringBuilder, method, callExpression);
		}

		private static Expression HandleAttributeValueCall(Expression callExpression)
		{
			return GetAttributeValue(callExpression);
		}

		public Expression HandleTextNode(TextNode textNode)
		{
			var text = Expression.Constant(textNode.Text);
			return GetText(text);
		}

		private static Expression GetText(Expression text)
		{
			var constructor = typeof (VText).GetConstructor(new[] {typeof (string)});

			return Expression.New(constructor, text);
		}

		public Expression HandleDocument(List<Expression> expressions)
		{
			var constructor = typeof(VNode).GetConstructor(new[] { typeof(IEnumerable<VTree>) });
			return Expression.New(constructor, CreateElementList(expressions));
		}

		public Expression HandleCompositeAttribute(CompositeAttributeContent compositeAttributeContent, INodeVisitor<Expression> visitor)
		{
			_stringBuilder = Expression.Variable(typeof (StringBuilder));
			var expressions = new List<Expression>();
			expressions.Add(Expression.Assign(_stringBuilder, Expression.New(typeof (StringBuilder))));

			_handleExpression = HandleCompositeAttributeValue;
            expressions.AddRange(compositeAttributeContent.ContentParts.Select(s => s.Accept(visitor)));

			var method = typeof (StringBuilder).GetMethod("ToString", new Type[0]);
			expressions.Add(Expression.Call(_stringBuilder, method));

			return GetAttributeValue(Expression.Block(new[] {_stringBuilder}, expressions));
		}
	}
}
