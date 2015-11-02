namespace TerrificNet.Thtml.Emit.Schema
{
	public abstract class DataSchema
	{
		public bool Nullable { get; }

		public static readonly DataSchema String = new SimpleDataSchema("String", true);
		public static readonly DataSchema Boolean = new SimpleDataSchema("Boolean", false);
		public static readonly DataSchema Any = new AnyDataSchema(true);

		protected DataSchema(bool nullable)
		{
			Nullable = nullable;
		}

		public abstract void Accept(IDataSchemaVisitor visitor);
	}
}