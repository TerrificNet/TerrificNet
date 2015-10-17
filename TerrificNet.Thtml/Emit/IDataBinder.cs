namespace TerrificNet.Thtml.Emit
{
    public interface IDataBinder
    {
        DataBinderResult Property(string propertyName);

        DataBinderResult Item();
        DataBinderResult Context();
    }
}