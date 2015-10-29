namespace TerrificNet.Thtml.Emit.Schema
{
	public class DataSchemaProperty
	{
		public string Name { get; }
		public DataSchema Schema { get; }

		public DataSchemaProperty(string name, DataSchema schema)
		{
			Name = name;
			Schema = schema;
		}
	}
}