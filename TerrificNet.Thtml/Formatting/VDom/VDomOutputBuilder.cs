using System.Collections.Generic;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Formatting.VDom
{
	public class VDomOutputBuilder : IOutputBuilder
	{
		private readonly IVDomBuilder _adaptee;

		public VDomOutputBuilder(IVDomBuilder adaptee)
		{
			_adaptee = adaptee;
			Inner = _adaptee;
		}

		internal IVDomBuilder Inner { get; }

		public void ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			_adaptee.ElementOpenStart(tagName);
			if (staticProperties != null)
			{
				foreach (var prop in staticProperties)
				{
					_adaptee.PropertyStart(prop.Key);
					_adaptee.Value(prop.Value);
					_adaptee.PropertyEnd();
				}
			}
		}

		public void ElementOpenEnd()
		{
			_adaptee.ElementOpenEnd();
		}

		public void ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			if (staticProperties.Count > 0)
			{
				ElementOpenStart(tagName, staticProperties);
				ElementOpenEnd();
			}
			else
				_adaptee.ElementOpen(tagName);
		}

		public void ElementClose(string tagName)
		{
			_adaptee.ElementClose();
		}

		public void PropertyStart(string propertyName)
		{
			_adaptee.PropertyStart(propertyName);
		}

		public void PropertyEnd()
		{
			_adaptee.PropertyEnd();
		}

		public void Value(string value)
		{
			_adaptee.Value(value);
		}
	}
}