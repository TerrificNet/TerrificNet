using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Schema;
using Xunit;

namespace TerrificNet.ViewEngine.Schema.Test
{
	
	public class SchemaCombinerTest
	{
		[Fact]
		public void TestTitleUsedFromAvailableSchema()
		{
			const string expectedResult = "title";

			var schema1 = new JSchema();
			var schema2 = new JSchema
			{
				Title = expectedResult
			};

			var underTest = new SchemaCombiner();
			var result = underTest.Apply(schema1, schema2, null);

			Assert.NotNull(result);
			Assert.Equal(expectedResult, result.Title);
		}

		[Fact]
		public void TestReportTitleConflictWhenBothHaveTitle()
		{
			var schema1 = new JSchema
			{
				Title = "s1"
			};
			var schema2 = new JSchema
			{
				Title = "s2"
			};

			var underTest = new SchemaCombiner();
			var report = new SchemaComparisionReport();
			var result = underTest.Apply(schema1, schema2, report);

			var failures = report.GetFailures().ToList();

			Assert.Equal(1, failures.Count);
            Assert.IsType(typeof(ValueConflict), failures[0]);

			var valueConflict = (ValueConflict)failures[0];
			Assert.Equal("title", valueConflict.PropertyName);
			Assert.Equal(schema1, valueConflict.SchemaPart);
			Assert.Equal(schema2, valueConflict.SchemaBasePart);
			Assert.Equal("s1", valueConflict.Value1);
			Assert.Equal("s2", valueConflict.Value2);
		}

		[Fact]
		public void TestCombineSchemaProperties()
		{
			var schema1 = new JSchema();
			schema1.Properties.Add("prop1", new JSchema());

			var schema2 = new JSchema();
			schema2.Properties.Add("prop2", new JSchema());

			var underTest = new SchemaCombiner();
			var result = underTest.Apply(schema1, schema2, null);

			Assert.NotNull(result);
			Assert.NotNull(result.Properties);
			Assert.Equal(2, result.Properties.Count);
			Assert.True(result.Properties.ContainsKey("prop1"), "Expect the property from the first schema.");
			Assert.True(result.Properties.ContainsKey("prop2"), "Expect the property from the second schema.");
		}

		[Fact]
		public void TestCombineSubSchemaProperties()
        {
            var schema1 = new JSchema();
            var propSchema1 = new JSchema();
            propSchema1.Properties.Add("sub_prop1", new JSchema());
            schema1.Properties.Add("prop1", propSchema1);

            var schema2 = new JSchema();
            var propSchema2 = new JSchema();
            propSchema2.Properties.Add("sub_prop2", new JSchema());
            schema2.Properties.Add("prop1", propSchema2);

            var underTest = new SchemaCombiner();
            var result = underTest.Apply(schema1, schema2, null);

            Assert.NotNull(result);
            Assert.NotNull(result.Properties);
            Assert.Equal(1, result.Properties.Count);
            Assert.True(result.Properties.ContainsKey("prop1"), "Expect the property from both schema.");

            var innerProperties = result.Properties["prop1"].Properties;
            Assert.Equal(2, innerProperties.Count);
            Assert.True(innerProperties.ContainsKey("sub_prop1"), "Expect the property from the first schema.");
            Assert.True(innerProperties.ContainsKey("sub_prop2"), "Expect the property from the second schema.");
        }
	}
}
