using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit
{
	public interface IHelperBinder
	{
		HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments);
	}
}