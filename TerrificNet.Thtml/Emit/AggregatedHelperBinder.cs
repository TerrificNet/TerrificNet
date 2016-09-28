using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit
{
	public class AggregatedHelperBinder : IHelperBinder
	{
		private readonly IHelperBinder[] _binders;

		public AggregatedHelperBinder(params IHelperBinder[] binders)
		{
			_binders = binders;
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