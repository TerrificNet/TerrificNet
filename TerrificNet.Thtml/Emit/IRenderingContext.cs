namespace TerrificNet.Thtml.Emit
{
	public interface IRenderingContext
	{
		bool TryGetData<T>(string key, out T obj);
	}
}