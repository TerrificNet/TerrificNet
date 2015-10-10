using System;

namespace TerrificNet.Thtml.Emit
{
    public interface IHelperBinder
    {
        HelperBinderResult FindByName(string helper);

    }

    public abstract class HelperBinderResult
    {
        public abstract IListEmitter<T> CreateEmitter<T>(IListEmitter<T> children);
    }
}