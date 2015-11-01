namespace TerrificNet.Thtml.Emit.Schema
{
	public class AnyDataSchema : DataSchema
	{
		public AnyDataSchema(bool nullable) : base(nullable)
		{
		}

		public override void Accept(IDataSchemaVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}