using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit.Schema
{
	public class BindingPath
	{
		public BindingPath(IEnumerable<string> segments)
		{
			Segments = segments.ToList();
		}

		public IEnumerable<string> Segments { get; }

		public override string ToString()
		{
			return string.Join(".", Segments);
		}
	}
}