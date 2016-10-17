using System.Collections.Generic;
using System.Linq.Expressions;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IOutputExpressionEmitter
	{
		Expression HandleAttributeContent(ConstantAttributeContent attributeContent);
		Expression HandleElement(Element element, INodeVisitor<Expression> visitor);
		Expression HandleElementList(IReadOnlyList<Expression> elements);
		IEnumerable<Expression> HandleAttributeNode(AttributeNode attributeNode, INodeVisitor<Expression> valueEmitter);
		Expression HandleCall(Expression callExpression);
		Expression HandleTextNode(TextNode textNode);
		Expression HandleDocument(List<Expression> expressions);
		Expression HandleCompositeAttribute(CompositeAttributeContent compositeAttributeContent, INodeVisitor<Expression> visitor);
	}
}