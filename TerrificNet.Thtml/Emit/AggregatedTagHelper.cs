using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	internal class AggregatedTagHelper : ITagHelper
	{
		private readonly IList<ITagHelper> _binders;

		public AggregatedTagHelper()
		{
			_binders = new List<ITagHelper>();
		}

		private AggregatedTagHelper(IEnumerable<ITagHelper> binders)
		{
			_binders = binders.ToList();
		}

		public AggregatedTagHelper AddHelper(ITagHelper binder)
		{
			return new AggregatedTagHelper(new[] { binder }.Union(_binders));
		}

		public HelperBinderResult FindByName(Element element)
		{
			foreach (var binder in _binders)
			{
				var result = binder.FindByName(element);
				if (result != null)
					return result;
			}

			return null;
		}
	}
}
