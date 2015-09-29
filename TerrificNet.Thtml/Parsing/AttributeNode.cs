using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Parsing
{
    public class ElementPart
    {
    }

    public class AttributeNode : ElementPart
    {
        public string Name { get; }
        public AttributeContent Value { get; }

        public AttributeNode(string name, AttributeContent value)
        {
            Name = name;
            Value = value;
        }

        public AttributeNode(string name, string value) : this(name, new ConstantAttributeContent(value))
        {
        }
    }

    public abstract class AttributeContent
    {
    }

    public class ConstantAttributeContent : AttributeContent
    {
        public string Text { get; private set; }

        public ConstantAttributeContent(string text)
        {
            Text = text;
        }
    }

    public class EvaluteExpressionAttributeContent : AttributeContent
    {
        public EvaluateExpression Expression { get; private set; }

        public EvaluteExpressionAttributeContent(EvaluateExpression expression)
        {
            Expression = expression;
        }
    }

    public class CompositeAttributeContent : AttributeContent
    {
        public AttributeContent[] ContentParts { get; private set; }

        public CompositeAttributeContent(params AttributeContent[] contentParts)
        {
            ContentParts = contentParts;
        }
    }
}