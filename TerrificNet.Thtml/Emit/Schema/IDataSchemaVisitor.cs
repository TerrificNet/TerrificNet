namespace TerrificNet.Thtml.Emit.Schema
{
	public interface IDataSchemaVisitor
	{
		void Visit(SimpleDataSchema simpleSchema);
		void Visit(AnyDataSchema anyDataSchema);
		void Visit(ComplexDataSchema complexDataSchema);
		void Visit(IterableDataSchema iterableDataSchema);
	}
}