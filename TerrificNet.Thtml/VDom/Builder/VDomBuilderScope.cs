using System;

namespace TerrificNet.Thtml.VDom.Builder
{
	internal abstract class VDomBuilderScope : IVDomBuilder
	{
		protected readonly VDomBuilder Builder;

		protected abstract string ScopeName { get; }

		protected VDomBuilderScope(VDomBuilder builder)
		{
			Builder = builder;
		}

		public virtual void ElementOpen(string tagName)
		{
			throw new InvalidOperationException($"Calling {nameof(ElementOpen)} inside {ScopeName} isn't allowed.");
		}

		public virtual void ElementClose()
		{
			throw new InvalidOperationException($"Calling {nameof(ElementClose)} inside {ScopeName} isn't allowed.");
		}

		public virtual void PropertyStart(string propertyName)
		{
			throw new InvalidOperationException($"Calling {nameof(PropertyStart)} inside {ScopeName} isn't allowed.");
		}

		public virtual void PropertyEnd()
		{
			throw new InvalidOperationException($"Calling {nameof(PropertyEnd)} inside {ScopeName} isn't allowed.");
		}

		public virtual void Value(object value)
		{
			throw new InvalidOperationException($"Calling {nameof(Value)} inside {ScopeName} isn't allowed.");
		}

		public virtual void ElementOpenStart(string tagName)
		{
			throw new InvalidOperationException($"Calling {nameof(ElementOpenStart)} inside {ScopeName} isn't allowed.");
		}

		public virtual void ElementOpenEnd()
		{
			throw new InvalidOperationException($"Calling {nameof(ElementOpenEnd)} inside {ScopeName} isn't allowed.");
		}
	}
}