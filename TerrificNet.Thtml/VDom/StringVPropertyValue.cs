namespace TerrificNet.Thtml.VDom
{
    public class StringVPropertyValue : VPropertyValue
    {

        public StringVPropertyValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

    }
}