using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Thtml.Emit
{
	public abstract class HelperBinderResult
	{
		public class HelperParameters
		{
			private IOutputExpressionEmitter _outputExpressionEmitter;
			private IHelperBinder _helperBinder;
			private IDataScopeContract _scopeContract;
			private ParameterExpression _dataContextParameter;

			public HelperParameters(IOutputExpressionEmitter outputExpressionEmitter, IHelperBinder helperBinder, IDataScopeContract scopeContract, ParameterExpression dataContextParameter)
			{
				_outputExpressionEmitter = outputExpressionEmitter;
				_helperBinder = helperBinder;
				_scopeContract = scopeContract;
				_dataContextParameter = dataContextParameter;
			}

			public IOutputExpressionEmitter OutputExpressionEmitter
			{
				get { return _outputExpressionEmitter; }
			}

			public IHelperBinder HelperBinder
			{
				get { return _helperBinder; }
			}

			public IDataScopeContract ScopeContract
			{
				get { return _scopeContract; }
			}

			public ParameterExpression DataContextParameter
			{
				get { return _dataContextParameter; }
			}
		}

		public abstract Expression CreateEmitter(HelperParameters helperParameters, Expression children);
	}
}