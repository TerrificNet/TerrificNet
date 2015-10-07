using System.Collections.Generic;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class EmitNodeVisitor : NodeVisitor
    {
        private readonly EmitExpressionVisitor _expressionVisitor;
        private readonly Stack<List<IListEmitter<VTree>>> _elements = new Stack<List<IListEmitter<VTree>>>();

        private List<IListEmitter<VTree>> Scope => _elements.Peek();

        public EmitNodeVisitor(IDataBinder dataBinder) : this(new EmitExpressionVisitor(dataBinder))
        {
        }

        private EmitNodeVisitor(EmitExpressionVisitor expressionVisitor) : base(expressionVisitor)
        {
            _expressionVisitor = expressionVisitor;
        }

        public IEmitter<VNode> DocumentFunc { get; private set; }

        public override bool BeforeVisit(Element element)
        {
            EnterScope();
            return true;
        }

        public override void AfterVisit(Element element)
        {
            var emitter = EmitterNode.Many(LeaveScope());
            Scope.Add(EmitterNode.AsList(EmitterNode.Lambda(d => new VElement(element.TagName, emitter.Execute(d)))));
        }

        public override void Visit(TextNode textNode)
        {
            Scope.Add(EmitterNode.AsList(EmitterNode.Lambda(d => new VText(textNode.Text))));
        }

        public override bool BeforeVisit(Document document)
        {
            EnterScope();
            return true;
        }

        public override void AfterVisit(Document document)
        {
            var emitter = EmitterNode.Many(LeaveScope());
            DocumentFunc = EmitterNode.Lambda(d => new VNode(emitter.Execute(d)));
        }

        public override bool BeforeVisit(Statement statement)
        {
            _expressionVisitor.EnterScope(statement.Expression);
            EnterScope();

            return true;
        }

        public override void AfterVisit(Statement statement)
        {
            var childEmitter = EmitterNode.Many(LeaveScope());
            var listEmitter = _expressionVisitor.LeaveScope(statement.Expression, childEmitter);

            Scope.Add(listEmitter);
        }

        private void EnterScope()
        {
            _elements.Push(new List<IListEmitter<VTree>>());
        }

        private List<IListEmitter<VTree>> LeaveScope()
        {
            return _elements.Pop();
        }

    }
}