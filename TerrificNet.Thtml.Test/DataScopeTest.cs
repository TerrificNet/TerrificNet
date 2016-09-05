using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Test.Asserts;
using TerrificNet.Thtml.Test.Stubs;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class DataScopeTest
	{
		private readonly DataScopeContract _underTest;
		private readonly BindingPathTemplate _global = BindingPathTemplate.Global;

		public DataScopeTest()
		{
			_underTest = new DataScopeContract(BindingPathTemplate.Global);
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
			var binding = _underTest.RequiresString();

			var schema = _underTest.CompleteSchema();
			Assert.Equal(DataSchema.String, schema);
			AssertBindingPath(binding, _global);
		}

		[Fact]
		public void TestBooleanScope()
		{
			var binding = _underTest.RequiresBoolean();

			var schema = _underTest.CompleteSchema();
			Assert.Equal(DataSchema.Boolean, schema);
			AssertBindingPath(binding, _global);
		}

		[Fact]
		public void TestEnumerableScope()
		{
			var expected = new IterableDataSchema(DataSchema.String, false);

			IDataScopeContract childScopeContract;
			var binding = _underTest.RequiresEnumerable(out childScopeContract);
			var childBinding = childScopeContract.RequiresString();

			var schema = _underTest.CompleteSchema();
			DataSchemaAssert.AssertSchema(expected, schema);

			AssertBindingPath(binding, _global);
			AssertBindingPath(childBinding, _global.Item());
		}

		[Fact]
		public void TestNullableChangeForIterableScope_ThrowsException()
		{
			_underTest.DependentNodes.Add(SyntaxNodeStub.Node1);

			IDataScopeContract childScopeContract;
			_underTest.RequiresEnumerable(out childScopeContract);

			var exception = Assert.Throws<DataContractException>(() => _underTest.RequiresBoolean());
			Assert.Equal(_underTest.DependentNodes, exception.DependentNodes);
		}

		[Fact]
		public void TestAccessPropertyOnString_ThrowsException()
		{
			_underTest.DependentNodes.Add(SyntaxNodeStub.Node1);

			_underTest.RequiresString();

			var exception = Assert.Throws<DataContractException>(() => _underTest.Property("test", SyntaxNodeStub.Node1));
			Assert.Equal(_underTest.DependentNodes, exception.DependentNodes);
		}

		[Fact]
		public void TestChangeFromBooleanToIterableScope()
		{
			var expected = new IterableDataSchema(DataSchema.Any, true);

			_underTest.RequiresBoolean();

			IDataScopeContract childScopeContract;
			_underTest.RequiresEnumerable(out childScopeContract);

			var schema = _underTest.CompleteSchema();
			DataSchemaAssert.AssertSchema(expected, schema);
		}

		[Fact]
		public void TestExtendIterableScope()
		{
			var expected = new IterableDataSchema(DataSchema.Any, new[]
			{
				new DataSchemaProperty("length", DataSchema.Any, new [] {SyntaxNodeStub.Node1 })
			}, false);

			IDataScopeContract childScopeContract;
			_underTest.RequiresEnumerable(out childScopeContract);
			_underTest.Property("length", SyntaxNodeStub.Node1);

			var schema = _underTest.CompleteSchema();
			DataSchemaAssert.AssertSchema(expected, schema);
		}

		[Fact]
		public void TestComplexToIterableScope()
		{
			var expected = new IterableDataSchema(DataSchema.Any, new[]
			{
				new DataSchemaProperty("length", DataSchema.Any, new [] {SyntaxNodeStub.Node1 })
			}, false);

			_underTest.Property("length", SyntaxNodeStub.Node1);

			IDataScopeContract childScopeContract;
			_underTest.RequiresEnumerable(out childScopeContract);

			var schema = _underTest.CompleteSchema();
			DataSchemaAssert.AssertSchema(expected, schema);
		}

		[Fact]
		public void TestChangeFromBooleanToStringScope()
		{
			var expected = DataSchema.String;

			_underTest.RequiresBoolean();
			_underTest.RequiresString();

			var schema = _underTest.CompleteSchema();
			DataSchemaAssert.AssertSchema(expected, schema);
		}

		[Fact]
		public void TestChangeFromStringToBooleanScope()
		{
			var expected = DataSchema.String;

			_underTest.RequiresString();
			_underTest.RequiresBoolean();

			var schema = _underTest.CompleteSchema();
			DataSchemaAssert.AssertSchema(expected, schema);
		}

		[Fact]
		public void TestDependentNodes()
		{
			var expected = new ComplexDataSchema(new[]
			{
				new DataSchemaProperty("prop1", DataSchema.Any, new [] {SyntaxNodeStub.Node1, SyntaxNodeStub.Node2 }),
				new DataSchemaProperty("prop2", DataSchema.Any, new [] {SyntaxNodeStub.Node3 }),
			}, false);

			_underTest.Property("prop1", SyntaxNodeStub.Node1);
			_underTest.Property("prop2", SyntaxNodeStub.Node3);
			_underTest.Property("prop1", SyntaxNodeStub.Node2);

			var schema = _underTest.CompleteSchema();
			DataSchemaAssert.AssertSchema(expected, schema);
		}

		[Fact]
		public void TestComplexSchemaScope()
		{
			const string propertyName = "test";

			var expected = new ComplexDataSchema(new[]
			{
				new DataSchemaProperty(propertyName, DataSchema.String, new [] {SyntaxNodeStub.Node1 })
			}, false);

			var scope = _underTest.Property(propertyName, SyntaxNodeStub.Node1);
			scope.RequiresString();
			var schema = _underTest.CompleteSchema();

			DataSchemaAssert.AssertSchema(expected, schema);
		}

		[Fact]
		public void DataScope_AccessProperty_ParentIsSetted()
		{
			var childScope = _underTest.Property("test", SyntaxNodeStub.Node1);

			Assert.Equal(_underTest, childScope.Parent);
		}

		[Fact]
		public void DataScope_AccessEnumerable_ParentIsSetted()
		{
			IDataScopeContract childContract;
			_underTest.RequiresEnumerable(out childContract);

			Assert.Equal(_underTest, childContract.Parent);
		}

		private static void AssertBindingPath<T>(IBinding<T> binding, BindingPathTemplate expected)
		{
			Assert.NotNull(binding);
			Assert.Equal(expected.ToString(), binding.Path.ToString());
		}
	}
}
