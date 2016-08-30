using System.IO;

namespace TerrificNet.Thtml.Rendering
{
	public interface IStreamRenderer
	{
		void Execute(TextWriter textWriter, object data, IRenderingContext renderingContext);
	}
}