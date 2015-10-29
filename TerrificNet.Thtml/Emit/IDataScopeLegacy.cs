using System.Collections;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit
{
	public interface IDataScopeLegacy
	{
		IDataScopeLegacy Property(string propertyName);

		IEvaluator<string> BindString();
		IEvaluator<bool> BindBoolean();
		IEvaluator<IEnumerable> BindEnumerable(out IDataScopeLegacy childScope);
	}

	public class DataScopeContractLegacyWrapper : IDataScopeContract
	{
		private readonly IDataScopeLegacy _legacy;

		public DataScopeContractLegacyWrapper(IDataScopeLegacy legacy)
		{
			_legacy = legacy;
		}

		public IDataScopeContract Property(string propertyName, SyntaxNode node)
		{
			return new DataScopeContractLegacyWrapper(_legacy.Property(propertyName));
		}

		public IEvaluator<string> RequiresString()
		{
			return _legacy.BindString();
		}

		public IEvaluator<bool> RequiresBoolean()
		{
			return _legacy.BindBoolean();
		}

		public IEvaluator<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			IDataScopeLegacy childScopeLegacy;
			var result = _legacy.BindEnumerable(out childScopeLegacy);
			childScopeContract = new DataScopeContractLegacyWrapper(childScopeLegacy);

			return result;
		}
	}
}