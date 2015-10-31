using System;
using System.Collections;
using System.Collections.Generic;
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
			IDataBinder childBinder;
			var result = _legacy.BindEnumerable(out childBinder);
			childScopeContract = new DataScopeContractLegacyWrapper(childBinder);

			return result;
		}
	}
}