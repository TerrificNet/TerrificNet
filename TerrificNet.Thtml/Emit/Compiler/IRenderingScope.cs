using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IRenderingScope
	{
		IEnumerable<IBinding> GetBindings();

		IRenderingScope Parent { get; }

		IReadOnlyList<IRenderingScope> Children { get; }

		bool IsEmpty();
	}
}