using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit
{
    public interface IListEmitter<out T> : IEmitterRunnable<IEnumerable<T>>
    {
    }
}