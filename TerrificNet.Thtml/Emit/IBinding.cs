using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Emit.Schema;

namespace TerrificNet.Thtml.Emit
{
	public interface IBinding
	{
		BindingPathTemplate Path { get; }

		bool IsSupported(RenderingScope server);
	}
}