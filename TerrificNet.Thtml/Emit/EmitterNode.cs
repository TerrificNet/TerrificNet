using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit
{
	public static class EmitterNode<T>
	{
		public static IListEmitter<T> Iterator(Func<object, IEnumerable> list, IListEmitter<T> blockEmitter)
		{
			return new IteratorEmitter(list, blockEmitter);
		}

		public static IListEmitter<T> Many(IEnumerable<IListEmitter<T>> emitters)
		{
			return new ManyEmitter(emitters);
		}

		public static IListEmitter<T> Condition(Func<object, bool> predicate, IListEmitter<T> blockEmitter)
		{
			return new ConditionalEmitter(predicate, blockEmitter);
		}

		public static IListEmitter<T> AsList(IEmitterRunnable<T> emitter)
		{
			return new ListEmitter(emitter);
		}

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

		private class ConditionalEmitter : IListEmitter<T>
		{
			private readonly Func<object, bool> _predicate;
			private readonly IListEmitter<T> _blockEmitter;

			public ConditionalEmitter(Func<object, bool> predicate, IListEmitter<T> blockEmitter)
			{
				_predicate = predicate;
				_blockEmitter = blockEmitter;
			}

			public IEnumerable<T> Execute(object context, IRenderingContext renderingContext)
			{
				bool result = _predicate(context);
				if (result)
					return _blockEmitter.Execute(context, renderingContext);

				return Enumerable.Empty<T>();
			}
		}

		private class IteratorEmitter : IListEmitter<T>
		{
			private readonly Func<object, IEnumerable> _list;
			private readonly IListEmitter<T> _blockEmitter;

			public IteratorEmitter(Func<object, IEnumerable> list, IListEmitter<T> blockEmitter)
			{
				_list = list;
				_blockEmitter = blockEmitter;
			}

			public IEnumerable<T> Execute(object context, IRenderingContext renderingContext)
			{
				return ExecuteInternal(context, renderingContext).SelectMany(d => d.ToList()).ToList();
			}

			private IEnumerable<IEnumerable<T>> ExecuteInternal(object context, IRenderingContext renderingContext)
			{
				var result = _list(context);
				if (result == null)
					yield break;

				foreach (var item in result)
				{
					yield return _blockEmitter.Execute(item, renderingContext).ToList();
				}
			}
		}

		private class ManyEmitter : IListEmitter<T>
		{
			private readonly IEnumerable<IListEmitter<T>> _emitters;

			public ManyEmitter(IEnumerable<IListEmitter<T>> emitters)
			{
				_emitters = emitters.ToList();
			}

			public IEnumerable<T> Execute(object context, IRenderingContext renderingContext)
			{
				return _emitters.SelectMany(e => e.Execute(context, renderingContext).ToList());
			}
		}

		private class ListEmitter : IListEmitter<T>
		{
			private readonly IList<IEmitterRunnable<T>> _emitter;

			public ListEmitter(IEmitterRunnable<T> emitter)
			{
				_emitter = new List<IEmitterRunnable<T>> { emitter };
			}

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