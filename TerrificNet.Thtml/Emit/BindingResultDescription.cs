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
			if (newValue != null && !(newValue is IEquatable<T>))
				return false;

			return IsMatch((IEquatable<T>) newValue);
		}

		public BindingResultDescription<T> Not(BindingResultDescription<T> other)
		{
			return new AndBindingResultDescription(this, new NotBindingResultDescription(other));
		}

		private class NotBindingResultDescription : BindingResultDescription<T>
		{
			private readonly BindingResultDescription<T> _expression;

			public NotBindingResultDescription(BindingResultDescription<T> expression)
			{
				_expression = expression;
			}

			public override bool IsMatch(IEquatable<T> value)
			{
				return !_expression.IsMatch(value);
			}
		}

		private class AndBindingResultDescription : BindingResultDescription<T>
		{
			private readonly BindingResultDescription<T> _desc1;
			private readonly BindingResultDescription<T> _desc2;

			public AndBindingResultDescription(BindingResultDescription<T> desc1, BindingResultDescription<T> desc2)
			{
				_desc1 = desc1;
				_desc2 = desc2;
			}

			public override bool IsMatch(IEquatable<T> value)
			{
				return _desc1.IsMatch(value) && _desc2.IsMatch(value);
			}
		}
	}
}