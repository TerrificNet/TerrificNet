using System.IO;
using TerrificNet.Thtml.Rendering;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public static class EmitterFactories
	{
		public static EmitterFactory<TextWriter> Stream { get; } = new EmitterFactory<TextWriter>(p => new StreamBuilderExpression(p));
		public static EmitterFactory<IVDomBuilder> VTree { get; } = new EmitterFactory<IVDomBuilder>(p => new VDomOutputExpressionBuilder(p));
		public static EmitterFactory<IIncrementalDomRenderer> IncrementalDomScript { get; } = new EmitterFactory<IIncrementalDomRenderer>(p => new IncrementalDomRendererExpressionBuilder(p));
	}
}