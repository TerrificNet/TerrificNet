using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class StreamOutputExpressionEmitter : IOutputExpressionEmitter
	{
		private readonly ParameterExpression _writerParameter;

		public StreamOutputExpressionEmitter(ParameterExpression writerParameter)
		{
			_writerParameter = writerParameter;
		}

		public Expression HandleAttributeContent(ConstantAttributeContent attributeContent)
		{
			return Write(attributeContent.Text);
		}

		public IEnumerable<Expression> HandleElement(Element element, INodeVisitor<Expression> visitor)
		{
			var expressions = new List<Expression>();
			expressions.Add(Write($"<{element.TagName}"));
			expressions.AddRange(element.Attributes.Select(attribute => attribute.Accept(visitor)));
			expressions.Add(Write(">"));
			expressions.AddRange(element.ChildNodes.Select(i => i.Accept(visitor)));
			expressions.Add(Write($"</{element.TagName}>"));
			return expressions;
		}

		public IEnumerable<Expression> HandleAttributeNode(AttributeNode attributeNode, Expression valueEmitter)
		{
			yield return Write(" " + attributeNode.Name + "=\"");
			yield return valueEmitter;
			yield return Write("\"");
		}

		public Expression HandleCall(Expression callExpression)
		{
			return ExpressionHelper.Write(_writerParameter, callExpression);
		}

		public Expression HandleTextNode(TextNode textNode)
		{
			return Write(textNode.Text);
		}

		private Expression Write(string value)
		{
			return ExpressionHelper.Write(_writerParameter, value);
		}
	}
}