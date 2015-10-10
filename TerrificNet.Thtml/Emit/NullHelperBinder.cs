using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit
{
    internal class NullHelperBinder : IHelperBinder
    {
        public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
        {
            return null;
        }
    }
}