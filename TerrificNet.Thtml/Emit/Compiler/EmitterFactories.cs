using System;
using System.IO;
using TerrificNet.Thtml.Rendering;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public static class EmitterFactories
	{
		public static IEmitterFactory<IViewTemplate<TextWriter>> Stream { get; } = new EmitterFactory<IViewTemplate<TextWriter>>(() => new Emitter<TextWriter>(p => new StreamBuilderExpression(p)));
		public static IEmitterFactory<IViewTemplate<IVDomBuilder>> VTree { get; } = new EmitterFactory<IViewTemplate<IVDomBuilder>>(() => new Emitter<IVDomBuilder>(p => new VDomOutputExpressionBuilder(p)));
		public static IEmitterFactory<IViewTemplate<IIncrementalDomRenderer>> IncrementalDomScript { get; } = new EmitterFactory<IViewTemplate<IIncrementalDomRenderer>>(() => new Emitter<IIncrementalDomRenderer>(p => new IncrementalDomRendererExpressionBuilder(p)));

		private class EmitterFactory<T> : IEmitterFactory<T>
		{
			private readonly Func<IEmitter<T>> _createFunc;

			public EmitterFactory(Func<IEmitter<T>> createFunc)
			{
				_createFunc = createFunc;
			}

			public IEmitter<T> Create()
			{
				return _createFunc();
			}
		}
	}
}