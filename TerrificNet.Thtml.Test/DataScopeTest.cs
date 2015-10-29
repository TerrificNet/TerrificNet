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

			Assert.NotNull(schema);
		}

		[Fact]
		public void TestStringScope()
		{
			_underTest.BindString();

			var schema = _underTest.GetSchema();
			Assert.Equal(DataSchema.String, schema);
		}

		[Fact]
		public void TestComplexSchemaScope()
		{
			const string propertyName = "test";

			var expected = new ComplexDataSchema(new[]
			{
				new DataSchemaProperty(propertyName, DataSchema.String)
			});

			var scope = _underTest.Property(propertyName);
			scope.BindString();
			var schema = _underTest.GetSchema();

			DataSchemaAssert.AssertSchema(expected, schema);
		}
	}
}
