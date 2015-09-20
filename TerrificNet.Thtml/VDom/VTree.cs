namespace TerrificNet.Thtml.VDom
{
    class VTree
    {
    }

    class VText : VTree
    {
        public VText(string text)
        {
            Text = text;
        }

        public string Text { get; }

    }

    class VNode : VTree
    {
        public VNode(string tagName)
        {
            TagName = tagName;
        }

        public string TagName { get; }

    }

    class VProperties
    {
    }

    class VProperty
    {

    }

    abstract class VPropertyValue
    {
    }

    class StringVPropertyValue : VPropertyValue
    {

        public StringVPropertyValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

    }

    class BooleanVPropertyValue : VPropertyValue
    {
        public BooleanVPropertyValue(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

    }

    class NumberVPropertyValue : VPropertyValue
    {
        public NumberVPropertyValue(int value)
        {
            Value = value;
        }

        public int Value { get; }

    }

}
