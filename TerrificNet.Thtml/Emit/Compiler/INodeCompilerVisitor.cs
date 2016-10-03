using System.Collections.Generic;
using System.Linq.Expressions;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface INodeCompilerVisitor : INodeVisitor<Expression>
	{
		INodeCompilerVisitor ChangeContract(IDataScopeContract childScopeContract);
		INodeCompilerVisitor ChangeExtensions(CompilerExtensions extensions);
		Expression Visit(IEnumerable<Node> nodes);
	}
}