using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal interface IOutputExpressionBuilder
	{
		Expression ElementOpenStart(string tagName);
		Expression ElementOpenEnd();
		Expression ElementOpen(string tagName);
		Expression ElementClose(string tagName);
		Expression PropertyStart(string propertyName);
		Expression PropertyEnd();
		Expression Value(Expression value);
	}
}