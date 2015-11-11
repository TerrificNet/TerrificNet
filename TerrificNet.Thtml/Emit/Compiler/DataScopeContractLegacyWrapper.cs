using System;
using System.Collections;
using System.Linq.Expressions;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class DataScopeContractLegacyWrapper : IDataScopeContract
	{
		private readonly IDataScopeContract _contract;
		private readonly IDataBinder _dataBinder;

		public DataScopeContractLegacyWrapper(IDataScopeContract contract, IDataBinder dataBinder)
		{
			_contract = contract;
			_dataBinder = dataBinder;
		}

		public IDataScopeContract Property(string propertyName, SyntaxNode node)
		{
			var propertyContract = _contract.Property(propertyName, node);
			return new DataScopeContractLegacyWrapper(propertyContract, _dataBinder.Property(propertyName));
		}

		public IBinding<string> RequiresString()
		{
			return new BindingWrapper<string>(d => _dataBinder.BindString(d));
		}

		public IBinding<bool> RequiresBoolean()
		{
			return new BindingWrapper<bool>(d => _dataBinder.BindBoolean(d));
		}

		public IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			var childBinder = _dataBinder.Item();
			IDataScopeContract childContract;
			_contract.RequiresEnumerable(out childContract);
			childScopeContract = new DataScopeContractLegacyWrapper(childContract, childBinder);

			return new BindingWrapper<IEnumerable>(d => _dataBinder.BindEnumerable(d));
		}

		public Type ResultType => _dataBinder.DataContextType;

		private class BindingWrapper<T> : IBinding<T>
		{
			private readonly Func<Expression, Expression> _createExpression;

			public BindingWrapper(Func<Expression, Expression> createExpression)
			{
				_createExpression = createExpression;
			}

			public void Train(Func<BindingResultDescriptionBuilder<T>, BindingResultDescription<T>> before, Func<BindingResultDescriptionBuilder<T>, BindingResultDescription<T>> after, string operation)
			{
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