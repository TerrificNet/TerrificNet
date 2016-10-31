using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal interface IBindingScope
	{
		IEnumerable<IBinding> GetBindings();

		IBindingScope Parent { get; }

		IReadOnlyList<IBindingScope> Children { get; }
	}
}