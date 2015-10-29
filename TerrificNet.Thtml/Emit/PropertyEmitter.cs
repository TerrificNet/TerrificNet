using System;
using System.Text;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class PropertyEmitter : EmitNodeVisitorBase<VProperty>
    {
        public PropertyEmitter(IDataScope dataScope, IHelperBinder helperBinder) : base(dataScope, helperBinder)
        {
        }

        public override IListEmitter<VProperty> Visit(AttributeNode attributeNode)
        {
            var valueVisitor = new PropertyValueEmitter(DataScope, HelperBinder);
            var valueEmitter = attributeNode.Value.Accept(valueVisitor);

            if (valueEmitter == null)
                valueEmitter = EmitterNode.AsList(EmitterNode.Lambda<VPropertyValue>((d, r) => null));

            return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VProperty(attributeNode.Name, GetPropertyValue(valueEmitter, d, r))));
        }

        public override IListEmitter<VProperty> Visit(AttributeStatement attributeStatement)
        {
            return HandleStatement(attributeStatement.Expression, attributeStatement.ChildNodes);
        }

        private static VPropertyValue GetPropertyValue(IListEmitter<VPropertyValue> emitter, IDataContext dataContext, IRenderingContext renderingContext)
        {
            var stringBuilder = new StringBuilder();
            foreach (var emit in emitter.Execute(dataContext, renderingContext))
            {
                var stringValue = emit as StringVPropertyValue;
                if (stringValue == null)
                    throw new Exception($"Unsupported property value {emit.GetType()}.");

                stringBuilder.Append(stringValue.Value);
            }

            return new StringVPropertyValue(stringBuilder.ToString());
        }

        protected override INodeVisitor<IListEmitter<VProperty>> CreateVisitor(IDataScope childScope)
        {
            return new PropertyEmitter(childScope, HelperBinder);
        }
    }
}