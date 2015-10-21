using System;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    public class VTreeEmitter : IEmitter<VTree>
    {
        public IEmitterRunnable<VTree> Emit(Document input, IDataBinder dataBinder, IHelperBinder helperBinder)
        {
            var visitor = new EmitNodeVisitor(dataBinder, helperBinder ?? new NullHelperBinder());
			visitor.Visit(input);
            return visitor.DocumentFunc;
        }
    }
}