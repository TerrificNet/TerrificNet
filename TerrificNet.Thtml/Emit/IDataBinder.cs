using System;
using System.Collections;
using System.Linq.Expressions;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	public interface IDataBinder
	{
		IDataBinder Property(string propertyName);

		IEvaluator<string> BindString();
		Expression BindStringToExpression(Expression dataContext);
		IEvaluator<bool> BindBoolean();
		IEvaluator<IEnumerable> BindEnumerable(out IDataBinder childScope);

		Type DataContextType { get; }
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
			return new BindingWrapper<string>(() => _legacy.BindString(), d => _legacy.BindStringToExpression(d));
		}

		public IBinding<bool> RequiresBoolean()
		{
			return new BindingWrapper<bool>(() => _legacy.BindBoolean(), null);
		}

		public IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			IDataBinder childBinder;
			var result = _legacy.BindEnumerable(out childBinder);
			IDataScopeContract childContract;
			_contract.RequiresEnumerable(out childContract);
            childScopeContract = new DataScopeContractLegacyWrapper(childContract, childBinder);

			return new BindingWrapper<IEnumerable>(() => result, null);
		}

		public Type ResultType => _legacy.DataContextType;

		private class BindingWrapper<T> : IBinding<T>
		{
			private readonly Func<IEvaluator<T>> _evalutor;
			private readonly Func<Expression, Expression> _createExpression;

			public BindingWrapper(Func<IEvaluator<T>> evalutor, Func<Expression, Expression> createExpression)
			{
				_evalutor = evalutor;
				_createExpression = createExpression;
			}

			public void Train(Func<ResultGenerator<T>, Result<T>> before, Func<ResultGenerator<T>, Result<T>> after, string operation)
			{
			}

			public IEvaluator<T> CreateEvaluator()
			{
				return _evalutor();
			}

			public Expression CreateExpression(Expression dataContext)
			{
				if (_createExpression == null)
					throw new NotImplementedException();

				return _createExpression(dataContext);
			}
		}
	}
}