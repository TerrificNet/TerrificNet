using TerrificNet.Thtml.Parsing;
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
}