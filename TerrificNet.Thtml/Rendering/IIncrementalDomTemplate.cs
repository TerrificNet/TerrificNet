namespace TerrificNet.Thtml.Rendering
{
	public interface IIncrementalDomTemplate
	{
		void Render(IIncrementalDomRenderer renderer, object data);
	}
}