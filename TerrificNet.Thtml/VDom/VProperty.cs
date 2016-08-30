namespace TerrificNet.Thtml.VDom
{
	public class VProperty
	{
		public string Name { get; }
		public VPropertyValue Value { get; }

		public VProperty(string name, VPropertyValue value)
		{
			Name = name;
			Value = value;
		}
	}
}