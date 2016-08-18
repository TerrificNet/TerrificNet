using System;
using TerrificNet.Thtml.Emit.Schema;

namespace TerrificNet.Thtml.Emit
{
	public abstract class BindingResultDescription<T> : IBindingResultDescription
	{
		internal BindingResultDescription()
		{
		}

	   public abstract bool IsMatch(IEquatable<T> value);

      public bool IsMatch(object newValue)
	   {
	      throw new NotImplementedException();
	   }

	   public BindingResultDescription<string> Not(BindingResultDescription<string> exact)
	   {
	      throw new NotImplementedException();
	   }
	}
}