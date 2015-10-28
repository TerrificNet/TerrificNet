using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    internal class PropertyEmitter : NodeVisitorBase<IListEmitter<VProperty>>
    {
        private readonly IDataBinder _dataBinder;

        public PropertyEmitter(IDataBinder dataBinder)
        {
            _dataBinder = dataBinder;
        }

        public override IListEmitter<VProperty> Visit(AttributeNode attributeNode)
        {
            var valueVisitor = new PropertyValueEmitter(_dataBinder);
            var valueEmitter = attributeNode.Value.Accept(valueVisitor);

            if (valueEmitter == null)
                valueEmitter = EmitterNode.Lambda<VPropertyValue>((d, r) => null);

            return EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VProperty(attributeNode.Name, valueEmitter.Execute(d, r))));
        }
    }
}