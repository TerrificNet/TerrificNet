using System.Collections;
using TerrificNet.Thtml.Emit;

namespace TerrificNet.Thtml.Test
{
	public class NullDataBinder : IDataBinder
	{
		public IDataBinder Property(string propertyName)
		{
			return null;
		}

		public IDataBinder Item()
		{
			return null;
		}

		public IEvaluator<string> BindString()
		{
			return null;
		}

		public IEvaluator<bool> BindBoolean()
		{
			return null;
		}

		public IEvaluator<IEnumerable> BindEnumerable(out IDataBinder childScope)
		{
			childScope = new NullDataBinder();
			return null;
		}
	}
}