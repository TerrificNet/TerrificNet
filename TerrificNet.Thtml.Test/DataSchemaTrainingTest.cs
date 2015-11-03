using System.Linq;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Test.Stubs;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class DataSchemaTrainingTest
	{
		[Fact]
		public void TestSetOperation()
		{
			var contract = new DataScopeContract("_global");
			contract.Property("p1", SyntaxNodeStub.Node1);
			var result = contract.RequiresString();

			result.Train(r => r.Exact(null), r => r.Any(), "Add node");
			result.Train(r => r.Any(), r => r.Exact(null), "Remove node x");
			result.Train(r => r.Any(), r => r.Any(), "Change node x");

			var schema = contract.CompleteSchema();
			var complexSchema = Assert.IsType<ComplexDataSchema>(schema);
			var property = complexSchema.Properties.FirstOrDefault(p => p.Name == "p1");
			Assert.NotNull(property);

			
		}
	}
}
