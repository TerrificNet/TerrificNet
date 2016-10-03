using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Emit
{
	public class HelperParameters
	{
		public HelperParameters(IDataScopeContract scopeContract, INodeCompilerVisitor visitor, CompilerExtensions compilerExtensions)
		{
			ScopeContract = scopeContract;
			Visitor = visitor;
			CompilerExtensions = compilerExtensions;
		}

		public IDataScopeContract ScopeContract { get; }

		public INodeCompilerVisitor Visitor { get; }

		public CompilerExtensions CompilerExtensions { get; }
	}
}