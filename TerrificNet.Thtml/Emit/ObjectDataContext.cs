namespace TerrificNet.Thtml.Emit
{
    public class ObjectDataContext : IDataContext
    {
        public ObjectDataContext(object value)
        {
            Value = value;
        }

        public object Value { get; }
    }
}