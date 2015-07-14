﻿using System.IO;
using System.Reflection;
using Moq;
using Newtonsoft.Json.Schema;
using TerrificNet.Test.Common;
using Veil.Handlebars;
using Veil.Helper;
using Xunit;
using Assert = Xunit.Assert;

namespace TerrificNet.ViewEngine.Schema.Test
{
    
    public class SchemaExtractorTest
    {
        /// <summary>
        /// Test whether a single property is included in the schema
        /// </summary>
        [Fact]
        public void TestSimpleSingleProperty()
        {
            var schemaExtractor = new SchemaExtractor(new HandlebarsParser());
            var schema = schemaExtractor.Run("test", new StreamReader(Path.Combine(PathUtility.GetDirectory(), "Mocks/simpleSingleProperty.mustache")), null, null);

            SchemaAssertions.AssertSingleProperty(schema, "Name", JSchemaType.String);
        }

        /// <summary>
        /// Test whether a single property is included in the schema
        /// </summary>
        [Fact]
        public void TestSimpleSinglePropertyPath()
        {
            var schemaExtractor = new SchemaExtractor(new HandlebarsParser());
            var schema = schemaExtractor.Run("test", new StreamReader(Path.Combine(PathUtility.GetDirectory(), "Mocks/simpleSinglePropertyPath.mustache")), null, null);

            SchemaAssertions.AssertSingleProperty(schema, "Customer", JSchemaType.Object);
            SchemaAssertions.AssertSingleProperty(schema.Properties["Customer"], "Name", JSchemaType.String);
        }

        /// <summary>
        /// Test whether a multiple properties are included in the schema
        /// </summary>
        [Fact]
        public void TestMultipleProperties()
        {
            var schemaExtractor = new SchemaExtractor(new HandlebarsParser());
            var schema = schemaExtractor.Run("test", new StreamReader(Path.Combine(PathUtility.GetDirectory(), "Mocks/multipleProperties.mustache")), null, null);

            SchemaAssertions.AssertSingleProperty(schema, "Title", JSchemaType.String);
            SchemaAssertions.AssertSingleProperty(schema, "Customer", JSchemaType.Object);
            SchemaAssertions.AssertSingleProperty(schema.Properties["Customer"], "Name", JSchemaType.String);
            SchemaAssertions.AssertSingleProperty(schema.Properties["Customer"], "Age", JSchemaType.String);
            SchemaAssertions.AssertSingleProperty(schema.Properties["Customer"], "Order", JSchemaType.Object);
            SchemaAssertions.AssertSingleProperty(schema.Properties["Customer"].Properties["Order"], "Count", JSchemaType.String);
        }

        /// <summary>
        /// A property inside a if expression is not required
        /// </summary>
        [Fact]
        public void TestNoRequiredPropertys()
        {
            var schemaExtractor = new SchemaExtractor(new HandlebarsParser());
            var schema = schemaExtractor.Run("test", new StreamReader(Path.Combine(PathUtility.GetDirectory(), "Mocks/noRequiredProperty.mustache")), null, null);

            SchemaAssertions.AssertSingleProperty(schema, "Customer", JSchemaType.Object);
            SchemaAssertions.AssertSingleProperty(schema.Properties["Customer"], "Name", JSchemaType.String, false);
        }

        /// <summary>
        /// A property only inside a if expression is a boolean
        /// </summary>
        [Fact]
        public void TestBooleanProperty()
        {
            var schemaExtractor = new SchemaExtractor(new HandlebarsParser());
            var schema = schemaExtractor.Run("test", new StreamReader(Path.Combine(PathUtility.GetDirectory(), "Mocks/booleanProperty.mustache")), null, null);

            SchemaAssertions.AssertSingleProperty(schema, "Customer", JSchemaType.Object);
            SchemaAssertions.AssertSingleProperty(schema.Properties["Customer"], "Name", JSchemaType.String);
            SchemaAssertions.AssertSingleProperty(schema.Properties["Customer"], "HasName", JSchemaType.Boolean, false);
        }

        /// <summary>
        /// The helper shoudn't be part of the schema.
        /// </summary>
        [Fact]
        public void TestIgnoreHelpers()
        {
            var schemaExtractor = new SchemaExtractor(new HandlebarsParser());

	        var helper = new Mock<IHelperHandler>();
	        helper.Setup(m => m.IsSupported(It.IsAny<string>())).Returns((string s) => s.StartsWith("helper"));

	        var helperHandlers = new [] { helper.Object };
            var schema = schemaExtractor.Run("test", new StreamReader(Path.Combine(PathUtility.GetDirectory(), "Mocks/ignoreHelpers.mustache")), null, helperHandlers);

            SchemaAssertions.AssertSingleProperty(schema, "Customer", JSchemaType.Object);
            SchemaAssertions.AssertSingleProperty(schema.Properties["Customer"], "Name", JSchemaType.String);

            Assert.False(schema.Properties.ContainsKey("helper param=\"val1\""), "No property helper should be inside the schema.");
            //Assert.True(schema.Properties.ContainsKey("noregistredHelper param=\"val1\""), "The none registred helpers should be still included.");
            SchemaAssertions.AssertSingleProperty(schema, "variableExpressionWithWhitespace", JSchemaType.String);
        }

        /// <summary>
        /// A property used inside a each expression is a array
        /// </summary>
        [Fact]
        public void TestArrayProperty()
        {
            var schemaExtractor = new SchemaExtractor(new HandlebarsParser());
            var schema = schemaExtractor.Run("test", new StreamReader(Path.Combine(PathUtility.GetDirectory(), "Mocks/arrayProperty.mustache")), null, null);

            SchemaAssertions.AssertSingleProperty(schema, "Customer", JSchemaType.Object);
            SchemaAssertions.AssertSingleProperty(schema.Properties["Customer"], "Addresses", JSchemaType.Array);
            Assert.NotNull(schema.Properties["Customer"].Properties["Addresses"].Items);// an items array should be given for an array type.
            Assert.Equal(1, schema.Properties["Customer"].Properties["Addresses"].Items.Count);// expectects exactly on item inside items.

            SchemaAssertions.AssertSingleProperty(schema.Properties["Customer"].Properties["Addresses"].Items[0], "Street", JSchemaType.String);
            SchemaAssertions.AssertSingleProperty(schema.Properties["Customer"].Properties["Addresses"].Items[0], "ZipCode", JSchemaType.String);
        }

        /// <summary>
        /// A property used inside a each expression is a array
        /// </summary>
        [Fact]
        public void TestArrayPropertyParent()
        {
            var schemaExtractor = new SchemaExtractor(new HandlebarsParser());
            var schema = schemaExtractor.Run("test", new StreamReader(Path.Combine(PathUtility.GetDirectory(), "Mocks/arrayPropertyParent.mustache")), null, null);

            SchemaAssertions.AssertSingleProperty(schema, "Customer", JSchemaType.Object);
            SchemaAssertions.AssertSingleProperty(schema, "Name", JSchemaType.String);
        }
    }
}
