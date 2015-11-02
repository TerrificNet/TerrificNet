using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit
{
    public class NullHelperBinder<TEmit, TConfig> : IHelperBinder<TEmit, TConfig>
    {
        public HelperBinderResult<TEmit, TConfig> FindByName(string helper, IDictionary<string, string> arguments)
        {
            return null;
        }
    }
}