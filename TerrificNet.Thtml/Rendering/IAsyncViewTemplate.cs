using System.Threading.Tasks;

namespace TerrificNet.Thtml.Rendering
{
	public interface IAsyncViewTemplate
	{
		Task Execute(object data, IRenderingContext renderingContext);
	}
}