namespace TerrificNet.Thtml.Emit
{
    internal class NullHelperBinder : IHelperBinder
    {
        public HelperBinderResult FindByName(string helper)
        {
            return null;
        }
    }
}