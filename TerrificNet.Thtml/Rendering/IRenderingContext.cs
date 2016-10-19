using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Rendering
{
	public interface IRenderingContext
	{
		IOutputBuilder OutputBuilder { get; }

		bool TryGetData<T>(string key, out T obj);
	}
}