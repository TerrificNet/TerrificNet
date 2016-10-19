using TerrificNet.Thtml.Formatting;

namespace TerrificNet.Thtml.Rendering
{
	public interface IRenderingContext
	{
		IOutputBuilder OutputBuilder { get; }

		bool TryGetData<T>(string key, out T obj);
	}
}