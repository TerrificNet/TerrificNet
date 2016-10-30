using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class DataScopeWithClientContract : IDataScopeContract
	{
		private readonly IDataScopeContract _contract;
		private readonly IDataScopeContract _clientScope;

		public DataScopeWithClientContract(IDataScopeContract contract, IDataScopeContract clientScope)
		{
			_contract = contract;
			_clientScope = clientScope;

			this.Path = BindingPathTemplate.Global;
		}

		public BindingPathTemplate Path { get; }

		public IDataScopeContract Property(string propertyName, SyntaxNode node)
		{
			if (propertyName == "$scope")
				return _clientScope;

			return _contract.Property(propertyName, node);
		}

		public IBinding RequiresString()
		{
			return _contract.RequiresString();
		}

		public IBinding RequiresBoolean()
		{
			return _contract.RequiresBoolean();
		}

		public IBinding RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			return _contract.RequiresEnumerable(out childScopeContract);
		}

		public IDataScopeContract Parent => null;
	}
}
