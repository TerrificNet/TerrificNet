using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
	public interface INodeVisitor<out T>
	{
		T Visit(Element element);
		T Visit(TextNode textNode);
		T Visit(Statement statement);
		T Visit(AttributeNode attributeNode);
		T Visit(AttributeContentStatement constantAttributeContent);
		T Visit(ConstantAttributeContent attributeContent);
		T Visit(Document document);
		T Visit(CompositeAttributeContent compositeAttributeContent);
		T Visit(CallHelperExpression callHelperExpression);
		T Visit(UnconvertedExpression unconvertedExpression);
		T Visit(AttributeStatement attributeStatement);
		T Visit(IterationExpression iterationExpression);
		T Visit(ConditionalExpression conditionalExpression);
		T Visit(MemberExpression memberExpression);
	}
}