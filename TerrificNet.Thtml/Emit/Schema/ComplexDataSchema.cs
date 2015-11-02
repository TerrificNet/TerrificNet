using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit.Schema
{
	public class ComplexDataSchema : DataSchema
	{
		public ComplexDataSchema(IEnumerable<DataSchemaProperty> properties, bool nullable) : base(nullable)
		{
			Properties = properties.ToList();
		}

		public IReadOnlyList<DataSchemaProperty> Properties { get; }

		public override void Accept(IDataSchemaVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}