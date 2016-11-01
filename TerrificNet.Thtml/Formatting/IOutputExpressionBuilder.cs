using System.Collections.Generic;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Formatting
{
	public interface IOutputExpressionBuilder : IBindingSupport
	{
		void ElementOpenStart(IExpressionBuilder expressionBuilder, string tagName, IReadOnlyDictionary<string, string> staticProperties);
		void ElementOpenEnd(IExpressionBuilder expressionBuilder);
		void ElementOpen(IExpressionBuilder expressionBuilder, string tagName, IReadOnlyDictionary<string, string> staticProperties);
		void ElementClose(IExpressionBuilder expressionBuilder, string tagName);
		void PropertyStart(IExpressionBuilder expressionBuilder, string propertyName);
		void PropertyEnd(IExpressionBuilder expressionBuilder);
		void Value(IExpressionBuilder expressionBuilder, IBinding valueBinding);
		void Text(IExpressionBuilder expressionBuilder, string text);
	}
}