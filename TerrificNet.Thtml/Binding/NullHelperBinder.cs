using System.Collections.Generic;
using TerrificNet.Thtml.Emit;

namespace TerrificNet.Thtml.Binding
{
	public class NullHelperBinder : IHelperBinder
	{
		public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
		{
			return null;
		}
	}
}