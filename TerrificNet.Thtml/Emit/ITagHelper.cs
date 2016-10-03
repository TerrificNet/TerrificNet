using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	public interface ITagHelper
	{
		HelperBinderResult FindByName(Element element);
	}
}
