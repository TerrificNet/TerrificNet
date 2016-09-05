using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit
{
	public abstract class HelperBinderResult
	{
		public abstract Expression CreateExpression(HelperParameters helperParameters, Expression children);
	}
}