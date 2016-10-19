using TerrificNet.Thtml.Formatting;

namespace TerrificNet.Thtml.Rendering
{
	public class RenderingContext : IRenderingContext
	{
		public RenderingContext(IOutputBuilder outputBuilder)
		{
			OutputBuilder = outputBuilder;
		}

		public IOutputBuilder OutputBuilder { get; }

		public bool TryGetData<T>(string key, out T obj)
		{
			obj = default(T);
			return false;
		}
	}
}