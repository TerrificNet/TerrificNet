using System;
using System.Text;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class PropertyEmitter : ListEmitNodeVisitor<VProperty>
	{
	    public PropertyEmitter(IDataScopeContract dataScopeContract, IHelperBinder<IListEmitter<VProperty>, object> helperBinder) 
			: base(dataScopeContract, helperBinder)
        {
        }

		public override IListEmitter<VProperty> Visit(AttributeNode attributeNode)
        {
            var valueVisitor = new PropertyValueEmitter(DataScopeContract, null);
            var valueEmitter = attributeNode.Value.Accept(valueVisitor);

            if (valueEmitter == null)
                valueEmitter = EmitterNode<VPropertyValue>.AsList(EmitterNode<VPropertyValue>.Lambda((d, r) => null));

            return EmitterNode<VProperty>.AsList(EmitterNode<VProperty>.Lambda((d, r) => new VProperty(attributeNode.Name, GetPropertyValue(valueEmitter, d, r))));
        }

        public override IListEmitter<VProperty> Visit(AttributeStatement attributeStatement)
        {
            return HandleStatement(attributeStatement.Expression, attributeStatement.ChildNodes);
        }

        private static VPropertyValue GetPropertyValue(IListEmitter<VPropertyValue> emitter, object dataContext, IRenderingContext renderingContext)
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

        protected override INodeVisitor<IListEmitter<VProperty>> CreateVisitor(IDataScopeContract childScopeContract)
        {
            return new PropertyEmitter(childScopeContract, HelperBinder);
        }
    }
}