namespace TerrificNet.Thtml.Parsing
{
    public interface INodeVisitor
    {
	    void Visit(Element element);
	    void Visit(TextNode textNode);
	    void Visit(Statement statement);
	    void Visit(AttributeNode attributeNode);
	    void Visit(AttributeContentStatement constantAttributeContent);
	    void Visit(ConstantAttributeContent attributeContent);
	    void Visit(Document document);
    }
}