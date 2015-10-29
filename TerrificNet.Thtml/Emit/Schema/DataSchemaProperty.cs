using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Schema
{
	public class DataSchemaProperty
	{
		public string Name { get; }
		public DataSchema Schema { get; }
		public IReadOnlyList<SyntaxNode> DependentNodes { get; }

		public DataSchemaProperty(string name, DataSchema schema, IEnumerable<SyntaxNode> dependentNodes)
		{
			Name = name;
			Schema = schema;
			DependentNodes = dependentNodes.ToList();
		}
	}
}