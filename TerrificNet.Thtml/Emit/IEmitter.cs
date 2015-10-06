namespace TerrificNet.Thtml.Emit
{
    public interface IEmitter<out T>
    {
        T Execute(IDataContext context);
    }
}