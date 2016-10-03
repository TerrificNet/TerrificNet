using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	public class HelperParameters
	{
		public HelperParameters(IDataScopeContract scopeContract, INodeVisitor<Expression> visitor, CompilerExtensions compilerExtensions)
		{
			ScopeContract = scopeContract;
			Visitor = visitor;
			CompilerExtensions = compilerExtensions;
		}

		public IDataScopeContract ScopeContract { get; }

		public INodeVisitor<Expression> Visitor { get; }

		public CompilerExtensions CompilerExtensions { get; }
	}
}