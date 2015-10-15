using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit
{
    public static class EmitterNode
    {
        public static IEmitter<T> Lambda<T>(Func<IDataContext, T> func)
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

            public IEnumerable<T> Execute(IDataContext context)
            {
                bool result = _predicate(context);
                if (result)
                    return _blockEmitter.Execute(context);

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

            public IEnumerable<T> Execute(IDataContext context)
            {
                return ExecuteInternal(context).SelectMany(d => d.ToList()).ToList();
            }

            private IEnumerable<IEnumerable<T>> ExecuteInternal(IDataContext context)
            {
                var result = _list(context);
                if (result == null)
                    yield break;

                foreach (var item in result)
                {
                    yield return _blockEmitter.Execute(new ObjectDataContext(item)).ToList();
                }
            }
        }

        private class ListEmitter<T> : IListEmitter<T>
        {
            private readonly IEmitter<T> _emitter;

            public ListEmitter(IEmitter<T> emitter)
            {
                _emitter = emitter;
            }

            public IEnumerable<T> Execute(IDataContext context)
            {
                yield return _emitter.Execute(context);
            }
        }

        private class LambdaEmitter<T> : IEmitter<T>
        {
            private readonly Func<IDataContext, T> _func;

            public LambdaEmitter(Func<IDataContext, T> func)
            {
                _func = func;
            }

            public T Execute(IDataContext context)
            {
                return _func(context);
            }
        }
        private class ManyEmitter<T> : IListEmitter<T>
        {
            private readonly IEnumerable<IListEmitter<T>> _emitters;

            public ManyEmitter(IEnumerable<IListEmitter<T>> emitters)
            {
                _emitters = emitters.ToList();
            }

            public IEnumerable<T> Execute(IDataContext context)
            {
                return _emitters.SelectMany(e => e.Execute(context).ToList());
            }
        }
    }
}