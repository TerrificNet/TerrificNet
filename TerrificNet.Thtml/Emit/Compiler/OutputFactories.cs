using System.IO;
using TerrificNet.Thtml.Rendering;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public static class OutputFactories
	{
		public static OutputExpressionBuilderFactory<TextWriter> Stream { get; } = new OutputExpressionBuilderFactory<TextWriter>(p => new StreamBuilderExpression(p));
		public static OutputExpressionBuilderFactory<IVDomBuilder> VTree { get; } = new OutputExpressionBuilderFactory<IVDomBuilder>(p => new VDomOutputExpressionBuilder(p));
		public static OutputExpressionBuilderFactory<IIncrementalDomRenderer> IncrementalDomScript { get; } = new OutputExpressionBuilderFactory<IIncrementalDomRenderer>(p => new IncrementalDomRendererExpressionBuilder(p));
	}
}