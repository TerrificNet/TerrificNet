using System;
using System.Collections.Generic;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Formatting.IncrementalDom
{
	public class IncrementalDomOutput : IOutputBuilder
	{
		public IncrementalDomOutput(IIncrementalDomRenderer adaptee)
		{
			Inner = adaptee;
		}

		internal IIncrementalDomRenderer Inner { get; }

		public void ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			throw new NotImplementedException();
		}

		public void ElementOpenEnd()
		{
			throw new NotImplementedException();
		}

		public void ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties)
		{
			throw new NotImplementedException();
		}

		public void ElementClose(string tagName)
		{
			throw new NotImplementedException();
		}

		public void PropertyStart(string propertyName)
		{
			throw new NotImplementedException();
		}

		public void PropertyEnd()
		{
			throw new NotImplementedException();
		}

		public void Value(string value)
		{
			throw new NotImplementedException();
		}
	}
}
