using System;
using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit
{
	public static class EmitterNode<T>
	{
		public static IListEmitter<T> AsList(IEnumerable<IEmitterRunnable<T>> emitter)
		{
			return new ListEmitter(emitter);
		}

		public static IEmitterRunnable<T> Lambda(Func<object, IRenderingContext, T> func)
		{
			return new LambdaEmitter(func);
		}

		private class LambdaEmitter : IEmitterRunnable<T>
		{
			private readonly Func<object, IRenderingContext, T> _func;

			public LambdaEmitter(Func<object, IRenderingContext, T> func)
			{
				_func = func;
			}

			public T Execute(object context, IRenderingContext renderingContext)
			{
				return _func(context, renderingContext);
			}
		}

		private class ListEmitter : IListEmitter<T>
		{
			private readonly IList<IEmitterRunnable<T>> _emitter;

			public ListEmitter(IEnumerable<IEmitterRunnable<T>> emitter)
			{
				_emitter = emitter.ToList();
			}

			public IEnumerable<T> Execute(object context, IRenderingContext renderingContext)
			{
				return _emitter.Select(e => e.Execute(context, renderingContext));
			}
		}
	}
}