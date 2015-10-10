namespace TerrificNet.Thtml.VDom
{
    public class BooleanVPropertyValue : VPropertyValue
    {
        public BooleanVPropertyValue(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

    }
}