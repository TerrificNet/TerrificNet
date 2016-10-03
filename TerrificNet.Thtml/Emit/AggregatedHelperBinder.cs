using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit
{
	internal class AggregatedHelperBinder : IHelperBinder
	{
		private readonly IList<IHelperBinder> _binders;

		public AggregatedHelperBinder()
		{
			_binders = new List<IHelperBinder>();
		}

		private AggregatedHelperBinder(IEnumerable<IHelperBinder> binders)
		{
			_binders = binders.ToList();
		}

		public AggregatedHelperBinder AddBinder(IHelperBinder binder)
		{
			return new AggregatedHelperBinder(new[] { binder }.Union(_binders));
		}

		public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
		{
			foreach (var binder in _binders)
			{
				var result = binder.FindByName(helper, arguments);
				if (result != null)
					return result;
			}

			return null;
		}
	}
}