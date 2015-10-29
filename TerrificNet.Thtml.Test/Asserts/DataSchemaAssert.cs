using TerrificNet.Thtml.Emit.Schema;
using Xunit;

namespace TerrificNet.Thtml.Test.Asserts
{
	public static class DataSchemaAssert
	{
		public static void AssertSchema(DataSchema expected, DataSchema actual)
		{
			if (expected == null)
			{
				Assert.Null(actual);
				return;
			}

			GenericAssert.AssertOneOf(
				() => GenericAssert.AssertValue<ComplexDataSchema>(expected, actual, AssertComplexDataSchem),
				() => GenericAssert.AssertValue<DataSchema>(expected, actual, AssertSimpleSchema));
		}

		private static void AssertSimpleSchema(DataSchema expected, DataSchema actual)
		{
		}

		private static void AssertComplexDataSchem(ComplexDataSchema expected, ComplexDataSchema actual)
		{
			GenericAssert.AssertCollection(expected.Properties, actual.Properties, AssertSchemaProperty);
		}

		private static void AssertSchemaProperty(DataSchemaProperty expected, DataSchemaProperty actual)
		{
			Assert.Equal(expected.Name, actual.Name);
			AssertSchema(expected.Schema, actual.Schema);
		}
	}
}
