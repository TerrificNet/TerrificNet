namespace TerrificNet.Mvc.Core
{
	public interface IViewDiscovery
	{
		string FindView(string viewName);
		string FindPartial(string partialName);
	}
}