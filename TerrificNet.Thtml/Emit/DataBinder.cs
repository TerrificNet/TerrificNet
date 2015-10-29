namespace TerrificNet.Thtml.Emit
{
	public abstract class DataBinder : IDataBinder
	{
		public abstract bool TryCreateEvaluation<T>(out IEvaluator<T> evaluationFunc);

		public abstract IDataBinder Property(string propertyName);

		public abstract IDataBinder Item();
	}
}