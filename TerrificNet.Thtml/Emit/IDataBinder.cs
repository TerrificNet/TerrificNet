using System;
using System.Collections;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	public interface IDataBinder
	{
		IDataBinder Property(string propertyName);

		IEvaluator<string> BindString();
		IEvaluator<bool> BindBoolean();
		IEvaluator<IEnumerable> BindEnumerable(out IDataBinder childScope);
		Type ResultType { get; }
	}

	public class DataScopeContractLegacyWrapper : IDataScopeContract
	{
		private readonly IDataBinder _legacy;

		public DataScopeContractLegacyWrapper(IDataBinder legacy)
		{
			_legacy = legacy;
		}

		public IDataScopeContract Property(string propertyName, SyntaxNode node)
		{
			return new DataScopeContractLegacyWrapper(_legacy.Property(propertyName));
		}

		public IBinding<string> RequiresString()
		{
			return new BindingWrapper<string>(_legacy.BindString());
		}

		public IBinding<bool> RequiresBoolean()
		{
			return new BindingWrapper<bool>(_legacy.BindBoolean());
		}

		public IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			IDataBinder childBinder;
			var result = _legacy.BindEnumerable(out childBinder);
			childScopeContract = new DataScopeContractLegacyWrapper(childBinder);

			return new BindingWrapper<IEnumerable>(result);
		}

		private class BindingWrapper<T> : IBinding<T>
		{
			private readonly IEvaluator<T> _evalutor;

			public BindingWrapper(IEvaluator<T> evalutor)
			{
				_evalutor = evalutor;
			}

			public T Evaluate(object context)
			{
				return _evalutor.Evaluate(context);
			}

			public void Train(Func<ResultGenerator<T>, Result<T>> before, Func<ResultGenerator<T>, Result<T>> after, string operation)
			{
			}
		}

		public Type ResultType => _legacy.ResultType;
	}
}