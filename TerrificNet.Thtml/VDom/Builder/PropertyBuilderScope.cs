using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrificNet.Thtml.VDom.Builder
{
	internal class PropertyBuilderScope : VDomBuilderScope
	{
		private string _propertyName;

		private readonly List<string> _stringList = new List<string>();

		public PropertyBuilderScope(VDomBuilder builder) : base(builder)
		{
		}

		protected override string ScopeName => "Element";

		public List<VProperty> CurrentValue { get; private set; }

		public override void ElementOpenStart(string tagName)
		{
			CurrentValue = new List<VProperty>();
		}

		public override void PropertyStart(string propertyName)
		{
			_propertyName = propertyName;
			_stringList.Clear();
		}

		public override void Value(object value)
		{
			_stringList.Add(value as string);
		}

		public override void PropertyEnd()
		{
			CurrentValue.Add(new VProperty(_propertyName, new StringVPropertyValue(GetValue(_stringList))));
		}

		public override void ElementOpenEnd()
		{
			Builder.ChangeScope(Builder.ListBuilderScope).ElementOpenEnd();
		}

		private static string GetValue(IReadOnlyList<string> stringList)
		{
			if (stringList.Count == 1)
				return stringList[0];

			if (stringList.Count == 0)
				return null;

			return stringList.Aggregate(new StringBuilder(), (a, b) => a.Append(b)).ToString();
		}
	}
}