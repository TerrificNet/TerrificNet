namespace TerrificNet.Thtml.VDom
{
    public abstract class VPropertyValue
    {
        public abstract void Accept(IVTreeVisitor visitor);

        public abstract object GetValue();
    }
}