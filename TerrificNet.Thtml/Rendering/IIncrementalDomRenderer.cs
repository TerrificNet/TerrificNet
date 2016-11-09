using System.Collections.Generic;

namespace TerrificNet.Thtml.Rendering
{
	public interface IIncrementalDomRenderer
	{
		void ElementVoid(string tagName, string key, IReadOnlyDictionary<string, string> staticPropertyValuePairs, IReadOnlyDictionary<string, string> propertyValuePairs);
		void ElementOpen(string tagName, string key, IReadOnlyDictionary<string, string> staticPropertyValuePairs, IReadOnlyDictionary<string, string> propertyValuePairs);
		void ElementOpenStart(string tagName, string key, IReadOnlyDictionary<string, string> staticPropertyValuePairs, IReadOnlyDictionary<string, string> propertyValuePairs);
		void Attr(string name, string value);
		void Attr(string name, ClientString value);
		void Text(string content);
		void Text(ClientString content);
		void ElementOpenEnd();
		void ElementClose(string tagName);
	}
}