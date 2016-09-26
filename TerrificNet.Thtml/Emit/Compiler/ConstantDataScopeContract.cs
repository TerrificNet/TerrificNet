using System;
using System.Collections;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class ConstantDataScopeContract : IDataScopeContract
	{
		private readonly object _value;
		public BindingPathTemplate Path { get; }
		public Expression Expression { get; }

		public ConstantDataScopeContract(object value)
		{
			_value = value;
		}

		public IDataScopeContract Property(string propertyName, SyntaxNode node)
		{
			throw new NotSupportedException();
		}

		public IBinding<string> RequiresString()
		{
			return new ConstantBinding<string>(Expression.Constant(_value));
		}

		public IBinding<bool> RequiresBoolean()
		{
			return new ConstantBinding<bool>(Expression.Constant(_value));
		}

		public IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			throw new NotSupportedException();
		}

		public IDataScopeContract Parent { get; }

		private class ConstantBinding<T> : IBinding<T>
		{
			public ConstantBinding(Expression expression)
			{
				Expression = expression;
			}

			public BindingPathTemplate Path { get; }
			public Expression Expression { get; }
		}
	}
}