namespace TerrificNet.Thtml.Emit
{
	public interface IEvaluator<out T>
	{
		T Evaluate(object context);
	}
}