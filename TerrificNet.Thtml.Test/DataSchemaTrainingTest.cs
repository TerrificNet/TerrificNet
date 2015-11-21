using System.Linq;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Test.Stubs;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class DataSchemaTrainingTest
	{
		[Fact(Skip = "Not implemented")]
		public void TestSetOperation()
		{
			var contract = new DataScopeContract(BindingPathTemplate.Global);
			var propertyContract = contract.Property("p1", SyntaxNodeStub.Node1);

			var propertyBinding = propertyContract.RequiresString();

			propertyBinding.Train(r => r.Exact(null), r => r.Any(), new DummyChangeOperation("Add node"));
			propertyBinding.Train(r => r.Any(), r => r.Exact(null), new DummyChangeOperation("Remove node x"));
			propertyBinding.Train(r => r.Any(), r => r.Any(), new DummyChangeOperation("Change node x"));

			var schema = contract.CompleteSchema();
			var complexSchema = Assert.IsType<ComplexDataSchema>(schema);
			var property = complexSchema.Properties.FirstOrDefault(p => p.Name == "p1");
			Assert.NotNull(property);

			var result = contract.PushChange(property, "hallo");
			Assert.NotNull(result);
			var resultList = result.ToList();
			Assert.Equal(1, resultList.Count);
			var dummyOp = Assert.IsType<DummyChangeOperation>(resultList[0]);
			Assert.Equal("Change node x", dummyOp.Text);
		}

		public class DummyChangeOperation : ChangeOperation
		{
			public string Text { get; }

			public DummyChangeOperation(string text)
			{
				Text = text;
			}
		}
	}
}
