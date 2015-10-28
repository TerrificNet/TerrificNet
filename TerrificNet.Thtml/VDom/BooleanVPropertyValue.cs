namespace TerrificNet.Thtml.VDom
{
    public class BooleanVPropertyValue : VPropertyValue
    {
        public BooleanVPropertyValue(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

        public override void Accept(IVTreeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override object GetValue()
        {
            return Value;
        }
    }
}