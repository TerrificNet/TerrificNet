using System.Collections;
using TerrificNet.Thtml.Emit;

namespace TerrificNet.Thtml.Test
{
	public class NullDataScope : IDataScope
	{
		public IDataScope Property(string propertyName)
		{
			return null;
		}

		public IDataScope Item()
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

		public IEvaluator<IEnumerable> BindEnumerable(out IDataScope childScope)
		{
			childScope = new NullDataScope();
			return null;
		}
	}
}