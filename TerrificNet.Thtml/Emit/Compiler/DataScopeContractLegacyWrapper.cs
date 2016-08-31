using System;
using System.Collections;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Schema;
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
			return new BindingWrapper<string>(_contract.RequiresString(), d => _dataBinder.BindString(d));
		}

		public IBinding<bool> RequiresBoolean()
		{
			return new BindingWrapper<bool>(_contract.RequiresBoolean(), d => _dataBinder.BindBoolean(d));
		}

		public IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			var childBinder = _dataBinder.Item();
			IDataScopeContract childContract;
			var binding = _contract.RequiresEnumerable(out childContract);
			childScopeContract = new DataScopeContractLegacyWrapper(childContract, childBinder);

			return new BindingWrapper<IEnumerable>(binding, d => _dataBinder.BindEnumerable(d));
		}

		public Type ResultType => _dataBinder.ResultType;

		private class BindingWrapper<T> : IBinding<T>
		{
			private readonly IBinding<T> _adaptee;
			private readonly Func<Expression, Expression> _createExpression;

			public BindingWrapper(IBinding<T> adaptee, Func<Expression, Expression> createExpression)
			{
				_adaptee = adaptee;
				_createExpression = createExpression;
			}

			public BindingPathTemplate Path => _adaptee.Path;

			public Expression CreateExpression(Expression dataContext)
			{
				return _createExpression(dataContext);
			}
		}
	}
}