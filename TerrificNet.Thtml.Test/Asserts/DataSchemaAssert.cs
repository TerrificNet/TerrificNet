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

			Assert.Equal(expected.Nullable, actual.Nullable);

			GenericAssert.AssertOneOf(
				() => GenericAssert.AssertValue<SimpleDataSchema>(expected, actual, AssertSimpleDataSchema),
				() => GenericAssert.AssertValue<IterableDataSchema>(expected, actual, AssertIterableDataSchema),
				() => GenericAssert.AssertValue<ComplexDataSchema>(expected, actual, AssertComplexDataSchema),
				() => GenericAssert.AssertValue<AnyDataSchema>(expected, actual, AssertEmptyDataSchema));
		}

		private static void AssertEmptyDataSchema(DataSchema expected, DataSchema actual)
		{
			Assert.Equal(expected, actual);
		}

		private static void AssertSimpleDataSchema(SimpleDataSchema expected, SimpleDataSchema actual)
		{
			Assert.Equal(expected.Name, actual.Name);
		}

		private static void AssertIterableDataSchema(IterableDataSchema expected, IterableDataSchema actual)
		{
			GenericAssert.AssertCollection(expected.Properties, actual.Properties, AssertSchemaProperty);
			AssertSchema(expected.ItemSchema, actual.ItemSchema);
		}

		private static void AssertComplexDataSchema(ComplexDataSchema expected, ComplexDataSchema actual)
		{
			GenericAssert.AssertCollection(expected.Properties, actual.Properties, AssertSchemaProperty);
		}

		private static void AssertSchemaProperty(DataSchemaProperty expected, DataSchemaProperty actual)
		{
			Assert.Equal(expected.Name, actual.Name);
			AssertSchema(expected.Schema, actual.Schema);

			Assert.Equal(expected.DependentNodes, actual.DependentNodes);
		}
	}
}
