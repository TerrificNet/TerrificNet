namespace TerrificNet.Thtml.VDom
{
	public class StringVPropertyValue : VPropertyValue
	{
		public StringVPropertyValue(string value)
		{
			Value = value;
		}

		public string Value { get; }

		public override void Accept(IVTreeVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override object GetValue()
		{
			return Value;
		}
	}
}