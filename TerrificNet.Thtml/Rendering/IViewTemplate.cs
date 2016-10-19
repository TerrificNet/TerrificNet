namespace TerrificNet.Thtml.Rendering
{
	public interface IViewTemplate
	{
		void Execute(object data, IRenderingContext renderingContext);
	}
}