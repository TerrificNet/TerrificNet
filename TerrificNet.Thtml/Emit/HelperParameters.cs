using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Emit
{
	public class HelperParameters
	{
		public HelperParameters(IDataScopeContract scopeContract, INodeCompilerVisitor visitor, CompilerExtensions compilerExtensions, Expression renderingContextExpression)
		{
			ScopeContract = scopeContract;
			Visitor = visitor;
			CompilerExtensions = compilerExtensions;
			RenderingContextExpression = renderingContextExpression;
		}

		public IDataScopeContract ScopeContract { get; }

		public INodeCompilerVisitor Visitor { get; }

		public CompilerExtensions CompilerExtensions { get; }

		public Expression RenderingContextExpression { get; }
	}
}