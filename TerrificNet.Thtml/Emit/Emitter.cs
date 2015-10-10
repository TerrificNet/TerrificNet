using System;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
    public class Emitter
    {
        public IEmitter<VTree> Emit(Document input, IDataBinder dataBinder, IHelperBinder helperBinder)
        {
            var visitor = new EmitNodeVisitor(dataBinder, helperBinder ?? new NullHelperBinder());
            input.Accept(visitor);
            return visitor.DocumentFunc;
        }
    }
}