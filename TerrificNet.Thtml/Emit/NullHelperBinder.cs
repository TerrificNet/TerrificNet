using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit
{
	public class NullHelperBinder : IHelperBinder
	{
		public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
		{
			return null;
		}
	}
}