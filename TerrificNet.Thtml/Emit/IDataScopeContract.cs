using System.Collections;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	public interface IDataScopeContract : IBinding
	{
		IDataScopeContract Property(string propertyName, SyntaxNode node);

		IBinding<string> RequiresString();
		IBinding<bool> RequiresBoolean();
		IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract);

		IDataScopeContract Parent { get; }
	}
}