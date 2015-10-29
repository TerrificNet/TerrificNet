using System;
using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit
{
    public interface IHelperBinder
    {
        HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments);
    }

    public abstract class HelperBinderResult
    {
        public HelperBinderResult()
        {
        }

        public abstract IListEmitter<T> CreateEmitter<T>(IListEmitter<T> children, IHelperBinder helperBinder, IDataScopeContract scopeContract);
    }
}