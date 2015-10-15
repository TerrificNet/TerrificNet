using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit
{
    public static class EmitterNode
    {
        public static IEmitter<T> Lambda<T>(Func<IDataContext, IRenderingContext, T> func)
        {
            return new LambdaEmitter<T>(func);
        }

        public static IListEmitter<T> Many<T>(IEnumerable<IListEmitter<T>> emitters)
        {
            return new ManyEmitter<T>(emitters);
        }

        public static IListEmitter<T> AsList<T>(IEmitter<T> emitter)
        {
            return new ListEmitter<T>(emitter);
        }

        public static IListEmitter<T> AsList<T>(IEnumerable<IEmitter<T>> emitter)
        {
            return new ListEmitter<T>(emitter);
        }

        public static IListEmitter<T> Iterator<T>(Func<IDataContext, IEnumerable> list, IListEmitter<T> blockEmitter)
        {
            return new IteratorEmitter<T>(list, blockEmitter);
        }

        public static IListEmitter<T> Condition<T>(Func<IDataContext, bool> predicate, IListEmitter<T> blockEmitter)
        {
            return new ConditionalEmitter<T>(predicate, blockEmitter);
        }

        private class ConditionalEmitter<T> : IListEmitter<T>
        {
            private readonly Func<IDataContext, bool> _predicate;
            private readonly IListEmitter<T> _blockEmitter;

            public ConditionalEmitter(Func<IDataContext, bool> predicate, IListEmitter<T> blockEmitter)
            {
                _predicate = predicate;
                _blockEmitter = blockEmitter;
            }

            public IEnumerable<T> Execute(IDataContext context, IRenderingContext renderingContext)
            {
                bool result = _predicate(context);
                if (result)
                    return _blockEmitter.Execute(context, renderingContext);

                return Enumerable.Empty<T>();
            }
        }

        private class IteratorEmitter<T> : IListEmitter<T>
        {
            private readonly Func<IDataContext, IEnumerable> _list;
            private readonly IListEmitter<T> _blockEmitter;

            public IteratorEmitter(Func<IDataContext, IEnumerable> list, IListEmitter<T> blockEmitter)
            {
                _list = list;
                _blockEmitter = blockEmitter;
            }

            public IEnumerable<T> Execute(IDataContext context, IRenderingContext renderingContext)
            {
                return ExecuteInternal(context, renderingContext).SelectMany(d => d.ToList()).ToList();
            }

            private IEnumerable<IEnumerable<T>> ExecuteInternal(IDataContext context, IRenderingContext renderingContext)
            {
                var result = _list(context);
                if (result == null)
                    yield break;

                foreach (var item in result)
                {
                    yield return _blockEmitter.Execute(new ObjectDataContext(item), renderingContext).ToList();
                }
            }
        }

        private class ListEmitter<T> : IListEmitter<T>
        {
            private readonly IList<IEmitter<T>> _emitter;

            public ListEmitter(IEmitter<T> emitter)
            {
                _emitter = new List<IEmitter<T>> { emitter };
            }

            public ListEmitter(IEnumerable<IEmitter<T>> emitter)
            {
                _emitter = emitter.ToList();
            }

            public IEnumerable<T> Execute(IDataContext context, IRenderingContext renderingContext)
            {
                return _emitter.Select(e => e.Execute(context, renderingContext));
            }
        }

        private class LambdaEmitter<T> : IEmitter<T>
        {
            private readonly Func<IDataContext, IRenderingContext, T> _func;

            public LambdaEmitter(Func<IDataContext, IRenderingContext, T> func)
            {
                _func = func;
            }

            public T Execute(IDataContext context, IRenderingContext renderingContext)
            {
                return _func(context, renderingContext);
            }
        }
        private class ManyEmitter<T> : IListEmitter<T>
        {
            private readonly IEnumerable<IListEmitter<T>> _emitters;

            public ManyEmitter(IEnumerable<IListEmitter<T>> emitters)
            {
                _emitters = emitters.ToList();
            }

            public IEnumerable<T> Execute(IDataContext context, IRenderingContext renderingContext)
            {
                return _emitters.SelectMany(e => e.Execute(context, renderingContext).ToList());
            }
        }
    }
}