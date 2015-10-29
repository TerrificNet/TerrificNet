namespace TerrificNet.Thtml.Emit.Schema
{
	public class IterableDataSchema : DataSchema
	{
		public DataSchema ItemSchema { get; }

		public IterableDataSchema(DataSchema itemSchema, bool nullable) : base(nullable)
		{
			ItemSchema = itemSchema;
		}
	}
}