using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Test.Asserts;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class DataScopeTest
	{
		private readonly DataScopeContract _underTest;

		private static readonly SyntaxNode Node1 = new DummySyntaxNode();
		private static readonly SyntaxNode Node2 = new DummySyntaxNode();
		private static readonly SyntaxNode Node3 = new DummySyntaxNode();

		private class DummySyntaxNode : SyntaxNode
		{
		}

		public DataScopeTest()
		{
			_underTest = new DataScopeContract("_global");
		}

		[Fact]
		public void TestEmptyScope()
		{
			var schema = _underTest.CompleteSchema();

			Assert.Equal(DataSchema.Any, schema);
		}

		[Fact]
		public void TestStringScope()
		{
			_underTest.RequiresString();

			var schema = _underTest.CompleteSchema();
			Assert.Equal(DataSchema.String, schema);
		}

		[Fact]
		public void TestBooleanScope()
		{
			_underTest.RequiresBoolean();

			var schema = _underTest.CompleteSchema();
			Assert.Equal(DataSchema.Boolean, schema);
		}

		[Fact]
		public void TestEnumerableScope()
		{
			var expected = new IterableDataSchema(DataSchema.String, false);

			IDataScopeContract childScopeContract;
			_underTest.RequiresEnumerable(out childScopeContract);
			childScopeContract.RequiresString();

			var schema = _underTest.CompleteSchema();
			DataSchemaAssert.AssertSchema(expected, schema);
		}

		[Fact]
		public void TestNullableChangeForIterableScope_ThrowsException()
		{
			_underTest.DependentNodes.Add(Node1);

			IDataScopeContract childScopeContract;
			_underTest.RequiresEnumerable(out childScopeContract);

			var exception = Assert.Throws<DataContextException>(() => _underTest.RequiresBoolean());
			Assert.Equal(_underTest.DependentNodes, exception.DependentNodes);
		}

		[Fact]
		public void TestChangeFromBooleanToIterableScope()
		{
			var expected = new IterableDataSchema(DataSchema.Any, true);

			var booleanEvaluator = _underTest.RequiresBoolean();

			IDataScopeContract childScopeContract;
			var enumEvaluator = _underTest.RequiresEnumerable(out childScopeContract);

			var schema = _underTest.CompleteSchema();
			DataSchemaAssert.AssertSchema(expected, schema);
		}

		[Fact]
		public void TestDependentNodes()
		{
			var expected = new ComplexDataSchema(new []
			{
				new DataSchemaProperty("prop1", DataSchema.Any, new [] { Node1, Node2 }),
				new DataSchemaProperty("prop2", DataSchema.Any, new [] { Node3 }),
			}, false);

			_underTest.Property("prop1", Node1);
			_underTest.Property("prop2", Node3);
			_underTest.Property("prop1", Node2);

			var schema = _underTest.CompleteSchema();
			DataSchemaAssert.AssertSchema(expected, schema);
		}

		[Fact]
		public void TestComplexSchemaScope()
		{
			const string propertyName = "test";

			var expected = new ComplexDataSchema(new[]
			{
				new DataSchemaProperty(propertyName, DataSchema.String, new [] { Node1 })
			}, false);

			var scope = _underTest.Property(propertyName, Node1);
			scope.RequiresString();
			var schema = _underTest.CompleteSchema();

			DataSchemaAssert.AssertSchema(expected, schema);
		}
	}
}
