namespace TerrificNet.Thtml.VDom
{
    public class NumberVPropertyValue : VPropertyValue
    {
        public NumberVPropertyValue(int value)
        {
            Value = value;
        }

        public int Value { get; }

    }
}