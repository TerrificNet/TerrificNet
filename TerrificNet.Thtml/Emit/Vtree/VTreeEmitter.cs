using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit.Vtree
{
	public class VTreeEmitter : IEmitter<VTree, IListEmitter<VTree>, object>
	{
		public IEmitterRunnable<VTree> Emit(Document input, IDataScopeContract dataScopeContract, IHelperBinder<IListEmitter<VTree>, object> helperBinder)
		{
			var visitor = new EmitNodeVisitor(dataScopeContract, helperBinder ?? new NullHelperBinder<IListEmitter<VTree>, object>());
			input.Accept(visitor);
			return visitor.DocumentFunc;
		}
	}
}