using System.Collections;

namespace TerrificNet.Thtml.Emit
{
	public interface IDataScope
	{
		IDataScope Property(string propertyName);

		IEvaluator<string> BindString();
		IEvaluator<bool> BindBoolean();
		IEvaluator<IEnumerable> BindEnumerable(out IDataScope childScope);
	}
}