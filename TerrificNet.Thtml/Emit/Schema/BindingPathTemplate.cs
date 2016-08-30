using System;
using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit.Schema
{
	public class BindingPathTemplate : IEquatable<BindingPathTemplate>
	{
		private readonly BindingPathTemplate _parent;

		private BindingPathTemplate(BindingPathTemplate parent)
		{
			_parent = parent;
			Segment = null;
		}

		public static readonly BindingPathTemplate Global = new BindingPathTemplate(null);

		public BindingPathTemplate Property(string property)
		{
			return new PropertyPathTemplate(property, this);
		}

		public BindingPathTemplate Item()
		{
			return new ItemPathTemplate(this);
		}

		protected virtual string Segment { get; }

		public bool Equals(BindingPathTemplate other)
		{
			return other != null && other.Segment == Segment && ((other._parent == null && _parent == null) || (other._parent != null && other._parent.Equals(_parent)));
		}

		public override bool Equals(object obj)
		{
			var tmp = obj as BindingPathTemplate;
			if (tmp == null)
				return false;

			return Segment == tmp.Segment && _parent == tmp._parent;
		}

		public static bool operator ==(BindingPathTemplate left, BindingPathTemplate right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(BindingPathTemplate left, BindingPathTemplate right)
		{
			return !Equals(left, right);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_parent?.GetHashCode() ?? 0)*397) ^ (Segment?.GetHashCode() ?? 0);
			}
		}

		public override string ToString()
		{
			var item = this;
			var segments = new List<string>();
			do
			{
				segments.Insert(0, item.Segment);
			} while ((item = item._parent) != null);

			return string.Join("/", segments.Where(s => s != null).ToArray());
		}

		private class PropertyPathTemplate : BindingPathTemplate
		{
			public PropertyPathTemplate(string segment, BindingPathTemplate parent) : base(parent)
			{
				Segment = segment;
			}

			protected override string Segment { get; }
		}

		private class ItemPathTemplate : BindingPathTemplate
		{
			public ItemPathTemplate(BindingPathTemplate parent) : base(parent)
			{
			}

			protected override string Segment => "[n]";
		}
	}
}