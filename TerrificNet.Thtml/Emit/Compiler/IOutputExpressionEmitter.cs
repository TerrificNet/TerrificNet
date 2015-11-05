using System.Collections.Generic;
using System.Linq.Expressions;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IOutputExpressionEmitter
	{
		Expression HandleAttributeContent(ConstantAttributeContent attributeContent);
		IEnumerable<Expression> HandleElement(Element element, INodeVisitor<Expression> visitor);
		IEnumerable<Expression> HandleAttributeNode(AttributeNode attributeNode, Expression valueEmitter);
		Expression HandleCall(Expression callExpression);
		Expression HandleTextNode(TextNode textNode);
	}
}