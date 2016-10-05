using System.Collections.Generic;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Mvc.Core
{
	public class SimpleHelperBinder : IHelperBinder
	{
		public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
		{
			return HelperBinderResult.Create(h => h.Visitor.Visit(new TextNode(helper)));
		}
	}
}