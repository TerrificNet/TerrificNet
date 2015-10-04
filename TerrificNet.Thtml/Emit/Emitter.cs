using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    public class Emitter
    {
        public IEmitter<VTree> Emit(Document input, IDataBinder data)
        {
            var visitor = new EmitNodeVisitor(data);
            input.Accept(visitor);
            return visitor.DocumentFunc;
        }
    }

    public interface IEmitter<out T>
    {
        T Execute(IDataContext context);
    }

    public interface IListEmitter<out T> : IEmitter<IEnumerable<T>>
    {
    }

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
    }

    internal class LambdaEmitter<T> : IEmitter<T>
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

    internal class ManyEmitter<T> : IListEmitter<T>
    {
        private readonly IEnumerable<IListEmitter<T>> _emitters;

        public ManyEmitter(IEnumerable<IListEmitter<T>> emitters)
        {
            _emitters = emitters;
        }

        public IEnumerable<T> Execute(IDataContext context)
        {
            return _emitters.SelectMany(e => e.Execute(context));
        }
    }

    public interface IDataContext
    {
        object Value { get; }
    }

    public class EmitNodeVisitor : NodeVisitor
    {
        private readonly IDataBinder _dataBinder;
        private readonly List<IListEmitter<VTree>> _elements = new List<IListEmitter<VTree>>();
        private Func<IDataContext, string> _value;

        public EmitNodeVisitor(IDataBinder dataBinder)
        {
            _dataBinder = dataBinder;
        }

        public IEmitter<VNode> DocumentFunc { get; private set; }

        public override void Visit(Element element)
        {
            var children = _elements.ToList();
            _elements.Clear();
            var emitter = EmitterNode.Many(children);
            _elements.Add(EmitterNode.AsList(EmitterNode.Lambda(d => new VElement(element.TagName, emitter.Execute(d)))));
        }

        public override void Visit(TextNode textNode)
        {
            _elements.Add(EmitterNode.AsList(EmitterNode.Lambda(d => new VText(textNode.Text))));
        }

        public override void Visit(Document document)
        {
            var emitter = EmitterNode.Many(_elements);
            DocumentFunc = EmitterNode.Lambda(d => new VNode(emitter.Execute(d)));
        }

        public override void Visit(Statement statement)
        {
            _elements.Add(EmitterNode.AsList(EmitterNode.Lambda(d => new VText(_value(d)))));
        }

        public override void Visit(MemberExpression memberExpression)
        {
            _value = _dataBinder.Evaluate(memberExpression);
        }
    }
}