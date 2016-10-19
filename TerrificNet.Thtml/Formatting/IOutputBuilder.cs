using System.Collections.Generic;

namespace TerrificNet.Thtml.Formatting
{
	public interface IOutputBuilder
	{
		void ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties);
		void ElementOpenEnd();
		void ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties);
		void ElementClose(string tagName);
		void PropertyStart(string propertyName);
		void PropertyEnd();
		void Value(string value);
	}
}