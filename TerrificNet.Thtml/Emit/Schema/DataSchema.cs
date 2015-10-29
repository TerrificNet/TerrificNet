namespace TerrificNet.Thtml.Emit.Schema
{
	public class DataSchema
	{
		public bool Nullable { get; }

		public static readonly DataSchema String = new SimpleDataSchema("String", true);
		public static readonly DataSchema Boolean = new SimpleDataSchema("Boolean", false);
		public static readonly DataSchema Empty = new DataSchema(true);

		protected DataSchema(bool nullable)
		{
			Nullable = nullable;
		}
	}
}