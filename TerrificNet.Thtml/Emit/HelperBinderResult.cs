using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Emit
{
	public abstract class HelperBinderResult
	{
		public class HelperParameters
		{
			public HelperParameters(IOutputExpressionEmitter outputExpressionEmitter, IHelperBinder helperBinder, IDataScopeContract scopeContract, ParameterExpression dataContextParameter)
			{
				OutputExpressionEmitter = outputExpressionEmitter;
				HelperBinder = helperBinder;
				ScopeContract = scopeContract;
				DataContextParameter = dataContextParameter;
			}

			public IOutputExpressionEmitter OutputExpressionEmitter { get; }

			public IHelperBinder HelperBinder { get; }

			public IDataScopeContract ScopeContract { get; }

			public ParameterExpression DataContextParameter { get; }
		}

		public abstract Expression CreateEmitter(HelperParameters helperParameters, Expression children);
	}
}