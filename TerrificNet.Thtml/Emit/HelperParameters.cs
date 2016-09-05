using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Emit
{
	public class HelperParameters
	{
		public HelperParameters(IOutputExpressionEmitter outputExpressionEmitter, IHelperBinder helperBinder, IDataScopeContract scopeContract)
		{
			OutputExpressionEmitter = outputExpressionEmitter;
			HelperBinder = helperBinder;
			ScopeContract = scopeContract;
		}

		public IOutputExpressionEmitter OutputExpressionEmitter { get; }

		public IHelperBinder HelperBinder { get; }

		public IDataScopeContract ScopeContract { get; }
	}
}