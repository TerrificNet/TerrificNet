namespace TerrificNet.Thtml.Emit
{
    public interface IDataBinder
    {
		IDataBinder Property(string propertyName);
		IDataBinder Item();
		IDataBinder Context();
	    bool TryCreateEvaluation<T>(out IEvaluator<T> evaluationFunc);
    }
}