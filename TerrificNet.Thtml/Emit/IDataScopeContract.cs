using System.Collections;
using System.Collections.Generic;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	public interface IDataScopeContract
	{
		IDataScopeContract Property(string propertyName, SyntaxNode node);

		IEvaluator<string> RequiresString();
		IEvaluator<bool> RequiresBoolean();
		IEvaluator<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract);
	}
}