using System.IO;

namespace TerrificNet.Mvc.Core
{
	internal class ViewDiscovery : IViewDiscovery
	{
		private readonly string _rootDir;

		public ViewDiscovery(string rootDir)
		{
			_rootDir = Path.Combine(rootDir, "Controllers");
		}

		public string FindView(string viewName)
		{
			return Path.Combine(_rootDir, string.Concat(viewName, ".html"));
		}

		public string FindPartial(string partialName)
		{
			return Path.Combine(_rootDir, string.Concat(partialName, ".html"));
		}
	}
}