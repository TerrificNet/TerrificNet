using System;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public static class EmitterFactories
	{
		public static IEmitterFactory<IStreamRenderer> Stream { get; } = new EmitterFactory<IStreamRenderer>(() => new StreamEmitter());
		public static IEmitterFactory<IVTreeRenderer> VTree { get; } = new EmitterFactory<IVTreeRenderer>(() => new VTreeEmitter());

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