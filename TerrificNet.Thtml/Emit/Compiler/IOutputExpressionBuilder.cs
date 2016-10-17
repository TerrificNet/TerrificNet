using System.Collections.Generic;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public interface IOutputExpressionBuilder
	{
		Expression ElementOpenStart(string tagName, IReadOnlyDictionary<string, string> staticProperties);
		Expression ElementOpenEnd();
		Expression ElementOpen(string tagName, IReadOnlyDictionary<string, string> staticProperties);
		Expression ElementClose(string tagName);
		Expression PropertyStart(string propertyName);
		Expression PropertyEnd();
		Expression Value(Expression value);
	}
}