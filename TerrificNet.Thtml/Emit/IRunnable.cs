namespace TerrificNet.Thtml.Emit
{
	public interface IRunnable<out T>
	{
		T Execute(object data, IRenderingContext renderingContext);
	}
}