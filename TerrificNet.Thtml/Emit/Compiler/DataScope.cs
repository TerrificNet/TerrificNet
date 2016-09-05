using System;
using System.Collections;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class DataScope : IDataScopeContract, IBinding
	{
		private readonly IDataScopeContract _contract;
		private readonly IDataBinder _dataBinder;
		private readonly DataScope _parent;

		public DataScope(IDataScopeContract contract, IDataBinder dataBinder, Expression expression, DataScope parent = null)
		{
			Expression = expression;
			_parent = parent;
			_contract = contract;
			_dataBinder = dataBinder;
		}

		public IDataScopeContract Property(string propertyName, SyntaxNode node)
		{
			var propertyContract = _contract.Property(propertyName, node);
			var dataBinder = _dataBinder.Property(propertyName);
			return new DataScope(propertyContract, dataBinder, Expression, this);
		}

		public IBinding<string> RequiresString()
		{
			return new BindingWrapper<string>(_contract.RequiresString(), d => _dataBinder.BindString(d), this);
		}

		public IBinding<bool> RequiresBoolean()
		{
			return new BindingWrapper<bool>(_contract.RequiresBoolean(), d => _dataBinder.BindBoolean(d), this);
		}

		public IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			var childBinder = _dataBinder.Item();
			IDataScopeContract childContract;
			var binding = _contract.RequiresEnumerable(out childContract);
			childScopeContract = new DataScope(childContract, childBinder, Expression.Parameter(childBinder.ResultType), _parent);

			return new BindingWrapper<IEnumerable>(binding, d => _dataBinder.BindEnumerable(d), this);
		}

		public IDataScopeContract Parent => _parent;

		private class BindingWrapper<T> : IBinding<T>
		{
			private readonly IBinding<T> _adaptee;

			public BindingWrapper(IBinding<T> adaptee, Func<Expression, Expression> createExpression, DataScope parent)
			{
				_adaptee = adaptee;
				this.Expression = createExpression(parent.Expression);
			}

			public BindingPathTemplate Path => _adaptee.Path;

			public Expression Expression { get; }
		}

		public BindingPathTemplate Path { get; }

		public Expression Expression { get; }
	}
}