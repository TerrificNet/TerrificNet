using System;
using System.Collections.Generic;
using System.Text;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class EmitNodeVisitor : NodeVisitor
    {
        private readonly EmitExpressionVisitor _expressionVisitor;
        private readonly Stack<List<IListEmitter<VTree>>> _elements = new Stack<List<IListEmitter<VTree>>>();
        private readonly List<IListEmitter<VProperty>> _properties = new List<IListEmitter<VProperty>>();
        private IEmitter<VPropertyValue> _propertyValueEmitter;

        private List<IListEmitter<VTree>> Scope => _elements.Peek();

        private EmitNodeVisitor(EmitExpressionVisitor expressionVisitor) : base(expressionVisitor)
        {
            _expressionVisitor = expressionVisitor;
        }

        public EmitNodeVisitor(IDataBinder dataBinder, IHelperBinder helperBinder) : this(new EmitExpressionVisitor(dataBinder, helperBinder))
        {
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
            var attributeEmitter = EmitterNode.Many(_properties);
            Scope.Add(EmitterNode.AsList(EmitterNode.Lambda(d => new VElement(element.TagName, attributeEmitter.Execute(d), emitter.Execute(d)))));
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
            var listEmitter = _expressionVisitor.LeaveTreeScope(statement.Expression, childEmitter);

            Scope.Add(listEmitter);
        }

        public override bool BeforeVisit(AttributeNode attributeNode)
        {
            return true;
        }

        public override void AfterVisit(AttributeNode attributeNode)
        {
            if (_propertyValueEmitter == null)
                _propertyValueEmitter = EmitterNode.Lambda<VPropertyValue>(d => null);

            var valueEmitter = _propertyValueEmitter;
            _properties.Add(EmitterNode.AsList(EmitterNode.Lambda(d => new VProperty(attributeNode.Name, valueEmitter.Execute(d)))));

            _propertyValueEmitter = null;
        }

        public override void Visit(AttributeContentStatement constantAttributeContent)
        {
            _expressionVisitor.EnterScope(constantAttributeContent.Expression);
            var emitter = _expressionVisitor.LeavePropertyValueScope(constantAttributeContent.Expression);

            _propertyValueEmitter = EmitterNode.Lambda(d => GetPropertyValue(emitter, d));
        }

        private static VPropertyValue GetPropertyValue(IListEmitter<VPropertyValue> emitter, IDataContext dataContext)
        {
            var stringBuilder = new StringBuilder();
            foreach (var emit in emitter.Execute(dataContext))
            {
                var stringValue = emit as StringVPropertyValue;
                if (stringValue == null)
                    throw new Exception($"Unsupported property value {emit.GetType()}.");

                stringBuilder.Append(stringValue.Value);
            }

            return new StringVPropertyValue(stringBuilder.ToString());
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