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

		public IDataBinder Context()
		{
			return null;
		}

		public bool TryCreateEvaluation<T>(out IEvaluator<T> evaluationFunc)
		{
			evaluationFunc = null;
			return false;
		}
	}
}