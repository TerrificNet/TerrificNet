namespace TerrificNet.Thtml.Emit.Schema
{
	public class SimpleDataSchema : DataSchema
	{
		public string Name { get; }

		public SimpleDataSchema(string name, bool nullable) : base(nullable)
		{
			Name = name;
		}
	}
}