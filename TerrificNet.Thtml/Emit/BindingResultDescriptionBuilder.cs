using System;

namespace TerrificNet.Thtml.Emit
{
	public class BindingResultDescriptionBuilder<T>
	{
		private static readonly AnyResultDescription AnyResultDescriptionField = new AnyResultDescription();
		private static readonly NullResultDescription NullResultDescriptionField = new NullResultDescription();

		public BindingResultDescription<T> Any => AnyResultDescriptionField;

		public BindingResultDescription<T> Exact(T value)
		{
			if (value == null)
				return NullResultDescriptionField;

			return new ExactResultDescription(value);
		}

		private class AnyResultDescription : BindingResultDescription<T>
		{
			public override bool IsMatch(IEquatable<T> value)
			{
				return true;
			}
		}

		private class ExactResultDescription : BindingResultDescription<T>
		{
			private readonly T _value;

			public ExactResultDescription(T value)
			{
				_value = value;
			}

			public override bool IsMatch(IEquatable<T> value)
			{
				return _value.Equals(value);
			}
		}

		private class NullResultDescription : BindingResultDescription<T>
		{
			public override bool IsMatch(IEquatable<T> value)
			{
				return value == null;
			}
		}
	}
}