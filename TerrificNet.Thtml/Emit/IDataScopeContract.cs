using System.Collections;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	public interface IDataScopeContract : IBinding
	{
		IDataScopeContract Property(string propertyName, SyntaxNode node);

		IBinding RequiresString();
		IBinding RequiresBoolean();
		IBinding RequiresEnumerable(out IDataScopeContract childScopeContract);

		IDataScopeContract Parent { get; }
	}
}