namespace TerrificNet.Thtml.Emit
{
	public abstract class DataBinderResult : IDataBinder
	{
		public abstract bool TryCreateEvaluation<T>(out IEvaluator<T> evaluationFunc);

		public abstract DataBinderResult Property(string propertyName);

		public abstract DataBinderResult Item();
		public DataBinderResult Context()
		{
			return this;
		}
	}
}