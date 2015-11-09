namespace TerrificNet.Thtml.Emit
{
	public interface IEmitterRunnable<out T>
	{
		T Execute(object data, IRenderingContext renderingContext);
	}
}