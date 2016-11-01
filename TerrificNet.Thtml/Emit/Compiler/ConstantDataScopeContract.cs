using System;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class ConstantDataScopeContract : IDataScopeContract
	{
		private readonly object _value;

		public BindingPathTemplate Path { get; }

		public ConstantDataScopeContract(object value)
		{
			_value = value;
		}

		public IDataScopeContract Property(string propertyName, SyntaxNode node)
		{
			throw new NotSupportedException();
		}

		public IBinding RequiresString()
		{
			return new ConstantBinding(_value);
		}

		public IBinding RequiresBoolean()
		{
			return new ConstantBinding(_value);
		}

		public IBinding RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			throw new NotSupportedException();
		}

		public IDataScopeContract Parent { get; }
	}
}