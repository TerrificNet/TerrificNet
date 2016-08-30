namespace TerrificNet.Thtml.Rendering
{
	public interface IRenderingContext
	{
		bool TryGetData<T>(string key, out T obj);
	}
}