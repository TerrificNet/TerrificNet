using Newtonsoft.Json.Schema;
using Xunit;

namespace TerrificNet.ViewEngine.Schema.Test
{
    public static class SchemaAssertions
    {
        public static void AssertSingleProperty(JSchema schema, string propertyName, JSchemaType schemaType, bool required = true)
        {
            Assert.NotNull(schema);
            Assert.NotNull(schema.Properties);
            Assert.True(schema.Properties.ContainsKey(propertyName), string.Format("property with name '{0}' expected.", propertyName));
            Assert.NotNull(schema.Properties[propertyName].Type);
            Assert.Equal(schemaType, schema.Properties[propertyName].Type.Value);
            if (required)
                Assert.True(schema.Required.Contains(propertyName), string.Format("Property {0} should be required", propertyName));
        }
    }
}
