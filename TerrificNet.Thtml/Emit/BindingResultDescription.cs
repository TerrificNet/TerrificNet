using System;

namespace TerrificNet.Thtml.Emit
{
	public abstract class BindingResultDescription<T>
	{
		internal BindingResultDescription()
		{
		}

	   public abstract bool IsMatch(IEquatable<T> value);
	}
}