namespace TerrificNet.Thtml.Rendering
{
	public interface IViewTemplate<in TRenderer>
	{
		void Execute(TRenderer renderer, object data, IRenderingContext renderingContext);
	}
}