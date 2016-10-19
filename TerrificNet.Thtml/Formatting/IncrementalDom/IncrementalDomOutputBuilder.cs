using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Formatting.IncrementalDom
{
	public class IncrementalDomOutputBuilder : IOutputBuilder
	{
		private string _currentProperty;
		private List<string> _propertyValues;

		public IncrementalDomOutputBuilder(IIncrementalDomRenderer adaptee)
		{
			Inner = adaptee;
		}

		internal IIncrementalDomRenderer Inner { get; }

		public void ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			Inner.ElementOpenStart(tagName, null, staticProperties, null);
		}

		public void ElementOpenEnd()
		{
			Inner.ElementOpenEnd();
		}

		public void ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			Inner.ElementOpen(tagName, null, staticProperties, null);
		}

		public void ElementClose(string tagName)
		{
			Inner.ElementClose(tagName);
		}

		public void PropertyStart(string propertyName)
		{
			_currentProperty = propertyName;
			_propertyValues = new List<string>();
		}

		public void PropertyEnd()
		{
			var value = _propertyValues.Aggregate(new StringBuilder(), (sb, a) => sb.Append(a)).ToString();
			Inner.Attr(_currentProperty, value);

			_currentProperty = null;
			_propertyValues = null;
		}

		public void Value(string value)
		{
			if (_currentProperty != null)
				_propertyValues.Add(value);
			else
				Inner.Text(value);
		}
	}
}
