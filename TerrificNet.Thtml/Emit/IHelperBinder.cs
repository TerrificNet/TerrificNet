using System;
using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit
{
	public interface IHelperBinder<TEmit, TConfig>
	{
		HelperBinderResult<TEmit, TConfig> FindByName(string helper, IDictionary<string, string> arguments);
	}
}