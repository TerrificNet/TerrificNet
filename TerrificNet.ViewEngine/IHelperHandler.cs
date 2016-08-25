using System.Collections.Generic;

namespace TerrificNet.ViewEngine
{
	public interface IHelperHandler
	{
		bool IsSupported(string name);
		void Evaluate(object model, RenderingContext context, IDictionary<string, string> parameters);
	}
}