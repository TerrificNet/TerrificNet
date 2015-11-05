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
		private readonly IDataScopeContract _contract;
		private readonly IDataBinder _legacy;

		public DataScopeContractLegacyWrapper(IDataScopeContract contract, IDataBinder legacy)
		{
			_contract = contract;
			_legacy = legacy;
		}

		public IDataScopeContract Property(string propertyName, SyntaxNode node)
		{
			var propertyContract = _contract.Property(propertyName, node);
			return new DataScopeContractLegacyWrapper(propertyContract, _legacy.Property(propertyName));
		}

		public IBinding<string> RequiresString()
		{
			return new BindingWrapper<string>(() => _legacy.BindString());
		}

		public IBinding<bool> RequiresBoolean()
		{
			return new BindingWrapper<bool>(() => _legacy.BindBoolean());
		}

		public IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			IDataBinder childBinder;
			var result = _legacy.BindEnumerable(out childBinder);
			IDataScopeContract childContract;
			_contract.RequiresEnumerable(out childContract);
            childScopeContract = new DataScopeContractLegacyWrapper(childContract, childBinder);

			return new BindingWrapper<IEnumerable>(() => result);
		}

		private class BindingWrapper<T> : IBinding<T>
		{
			private readonly Func<IEvaluator<T>> _evalutor;

			public BindingWrapper(Func<IEvaluator<T>> evalutor)
			{
				_evalutor = evalutor;
			}

			public void Train(Func<ResultGenerator<T>, Result<T>> before, Func<ResultGenerator<T>, Result<T>> after, string operation)
			{
			}

			public IEvaluator<T> CreateEvaluator()
			{
				return _evalutor();
			}
		}

		public Type ResultType => _legacy.ResultType;
	}
}