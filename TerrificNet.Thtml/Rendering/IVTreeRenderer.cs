using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Rendering
{
	public interface IVTreeRenderer
	{
		VTree Execute(object data, IRenderingContext renderingContext);
	}
}