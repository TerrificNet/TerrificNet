using System;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Emit
{
	public class VTreeEmitter : IEmitter<VTree>
	{
		public IEmitterRunnable<VTree> Emit(Document input, IDataScope dataScope, IHelperBinder helperBinder)
		{
			var visitor = new EmitNodeVisitor(dataScope, helperBinder ?? new NullHelperBinder());
			input.Accept(visitor);
			return visitor.DocumentFunc;
		}
	}
}