using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Test.Asserts;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class DataScopeTest
	{
		private readonly DataScope _underTest;

		public DataScopeTest()
		{
			_underTest = new DataScope();
		}

		[Fact]
		public void TestEmptyScope()
		{
			var schema = _underTest.GetSchema();

			Assert.Equal(DataSchema.Empty, schema);
		}

		[Fact]
		public void TestStringScope()
		{
			_underTest.BindString();

			var schema = _underTest.GetSchema();
			Assert.Equal(DataSchema.String, schema);
		}

		[Fact]
		public void TestBooleanScope()
		{
			_underTest.BindBoolean();

			var schema = _underTest.GetSchema();
			Assert.Equal(DataSchema.Boolean, schema);
		}

		[Fact]
		public void TestEnumerableScope()
		{
			var expected = new IterableDataSchema(DataSchema.String, false);

			IDataScope childScope;
			_underTest.BindEnumerable(out childScope);
			childScope.BindString();

			var schema = _underTest.GetSchema();
			DataSchemaAssert.AssertSchema(expected, schema);
		}

		[Fact]
		public void TestNullableChangeForIterableScope_ThrowsException()
		{
			IDataScope childScope;
			_underTest.BindEnumerable(out childScope);

			Assert.Throws<ContextException>(() => _underTest.BindBoolean());
		}

		[Fact]
		public void TestChangeFromBooleanToIterableScope()
		{
			var expected = new IterableDataSchema(DataSchema.Empty, true);

			_underTest.BindBoolean();

			IDataScope childScope;
			_underTest.BindEnumerable(out childScope);

			var schema = _underTest.GetSchema();
			DataSchemaAssert.AssertSchema(expected, schema);
		}

		[Fact]
		public void TestComplexSchemaScope()
		{
			const string propertyName = "test";

			var expected = new ComplexDataSchema(new[]
			{
				new DataSchemaProperty(propertyName, DataSchema.String)
			}, false);

			var scope = _underTest.Property(propertyName);
			scope.BindString();
			var schema = _underTest.GetSchema();

			DataSchemaAssert.AssertSchema(expected, schema);
		}
	}
}
