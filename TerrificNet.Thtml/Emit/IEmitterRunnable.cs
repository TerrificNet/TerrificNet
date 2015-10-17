using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
	public interface IEmitterRunnable<out T>
	{
		T Execute(IDataContext context, IRenderingContext renderingContext);
	}
}