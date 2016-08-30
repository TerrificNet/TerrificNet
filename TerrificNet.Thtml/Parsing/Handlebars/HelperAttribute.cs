namespace TerrificNet.Thtml.Parsing.Handlebars
{
	public class HelperAttribute
	{
		public string Name { get; }
		public string Value { get; }

		public HelperAttribute(string name, string value)
		{
			Name = name;
			Value = value;
		}
	}
}