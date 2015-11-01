using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit.Schema
{
	public class IterableDataSchema : ComplexDataSchema
	{
		public DataSchema ItemSchema { get; }

		public IterableDataSchema(DataSchema itemSchema, bool nullable)
			: this(itemSchema, Enumerable.Empty<DataSchemaProperty>(), nullable)
		{
		}

		public IterableDataSchema(DataSchema itemSchema, IEnumerable<DataSchemaProperty> properties, bool nullable) : base(properties, nullable)
		{
			ItemSchema = itemSchema;
		}

		public override void Accept(IDataSchemaVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}