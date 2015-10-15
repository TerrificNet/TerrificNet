namespace TerrificNet.Thtml.Emit
{
    public interface IEmitter<out T>
    {
        T Execute(IDataContext context, IRenderingContext renderingContext);
    }

    public interface IRenderingContext
    {
        bool TryGetData<T>(string key, out T obj);
    }
}