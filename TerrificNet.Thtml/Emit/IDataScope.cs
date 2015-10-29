using System.Collections;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	public interface IDataScope
	{
		IDataScope Property(string propertyName, SyntaxNode node);

		IEvaluator<string> BindString();
		IEvaluator<bool> BindBoolean();
		IEvaluator<IEnumerable> BindEnumerable(out IDataScope childScope);
	}
}