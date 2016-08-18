using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Test.Stubs;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class DataSchemaTrainingTest
	{
		[Theory]
		[InlineData("before", "after", "Change node x")]
		[InlineData(null, "after", "Add node")]
		[InlineData("before", null, "Remove node x")]
		public void TestSetOperation(string oldValue, string newValue, string expectedResult)
		{
			var contract = new DataScopeContract(BindingPathTemplate.Global);
			var propertyContract = contract.Property("p1", SyntaxNodeStub.Node1);

			var propertyBinding = propertyContract.RequiresString();

			propertyBinding.Train(r => r.Exact(null), r => r.Any, new ChangeOperationDummy("Add node"));
			propertyBinding.Train(r => r.Any.Not(r.Exact(null)), r => r.Exact(null), new ChangeOperationDummy("Remove node x"));
			propertyBinding.Train(r => r.Any.Not(r.Exact(null)), r => r.Any.Not(r.Exact(null)), new ChangeOperationDummy("Change node x"));

			var result = contract.PushChange(BindingPathTemplate.Global.Property("p1"), oldValue, newValue);
			AssertResult(expectedResult, result);
		}

		private static void AssertResult(string expectedResult, IEnumerable<ChangeOperation> result)
		{
			Assert.NotNull(result);
			var resultList = result.ToList();
			Assert.Equal(1, resultList.Count);
			var dummyOp = Assert.IsType<ChangeOperationDummy>(resultList[0]);
			Assert.Equal(expectedResult, dummyOp.Text);
		}

		[Fact]
		public void TestCollectionOperation_Add()
		{
			var contract = SetupCollectionOperationTest();

			var result = contract.PushChangeAddNode(BindingPathTemplate.Global, "node");
			AssertResult("Add node", result);
		}

		[Fact]
		public void TestCollectionOperation_Remove()
		{
			var contract = SetupCollectionOperationTest();

			var result = contract.PushChangeRemoveNode(BindingPathTemplate.Global, "node");
			AssertResult("Remove node", result);
		}

		[Fact]
		public void TestCollectionOperation_Move()
		{
			var contract = SetupCollectionOperationTest();

			var result = contract.PushChangeMoveNode(BindingPathTemplate.Global, BindingPathTemplate.Global, "node");
			AssertResult("Move node", result);
		}

		private static DataScopeContract SetupCollectionOperationTest()
		{
			var contract = new DataScopeContract(BindingPathTemplate.Global);
			IDataScopeContract childContract;
			var binding = contract.RequiresEnumerable(out childContract);

			binding.TrainRemove(new ChangeOperationDummy("Remove node"));
			binding.TrainAdd(new ChangeOperationDummy("Add node"));
			binding.TrainMove(new ChangeOperationDummy("Move node"));
			return contract;
		}

		[Fact]
		public void TestBindingResultDescription()
		{
			var desc = new BindingResultDescriptionBuilder<string>();
			var result = desc.Exact("hallo");

			Assert.True(result.IsMatch("hallo"));
			Assert.False(result.IsMatch("hallo2"));
		}

		[Fact]
		public void TestBindingResultDescriptionNull()
		{
			var desc = new BindingResultDescriptionBuilder<string>();
			var result = desc.Exact(null);

			Assert.True(result.IsMatch(null));
			Assert.False(result.IsMatch("hallo2"));
		}

		[Fact]
		public void TestBindingResultDescriptionNotNull()
		{
			var desc = new BindingResultDescriptionBuilder<string>();
			var result = desc.Any.Not(desc.Exact(null));

			Assert.True(result.IsMatch("hallo"));
			Assert.False(result.IsMatch(null));
		}

		private class ChangeOperationDummy : ChangeOperation
		{
			public string Text { get; }

			public ChangeOperationDummy(string text)
			{
				Text = text;
			}
		}
	}
}
