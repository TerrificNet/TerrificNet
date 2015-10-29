using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit.Schema
{
	public class ComplexDataSchema : DataSchema
	{
		public ComplexDataSchema(IEnumerable<DataSchemaProperty> properties)
		{
			Properties = properties.ToList();
		}

		public IReadOnlyList<DataSchemaProperty> Properties { get; }
	}
}