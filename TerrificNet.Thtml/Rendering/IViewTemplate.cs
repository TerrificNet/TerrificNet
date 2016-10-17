namespace TerrificNet.Thtml.Rendering
{
	public interface IViewTemplate
	{
		void Execute(object renderer, object data, IRenderingContext renderingContext);
	}

	public interface IViewTemplate<in TRenderer> : IViewTemplate
	{
		void Execute(TRenderer renderer, object data, IRenderingContext renderingContext);
	}
}