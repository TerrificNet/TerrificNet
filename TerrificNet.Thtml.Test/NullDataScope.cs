using System;
using System.Collections;
using System.Collections.Generic;
using TerrificNet.Thtml.Emit;

namespace TerrificNet.Thtml.Test
{
	public class NullDataScope : IDataScopeLegacy
	{
		public IDataScopeLegacy Property(string propertyName)
		{
			return null;
		}

		public IDataScopeLegacy Item()
		{
			return null;
		}

		public IEvaluator<string> BindString()
		{
			return null;
		}

		public IEvaluator<bool> BindBoolean()
		{
			return null;
		}

		public IEvaluator<IEnumerable> BindEnumerable(out IDataScopeLegacy childScope)
		{
			childScope = new NullDataScope();
			return null;
		}

		public Type ResultType { get; } = typeof(object);
	}
}