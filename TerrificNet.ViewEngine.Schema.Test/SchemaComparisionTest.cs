using System.Linq;
using Newtonsoft.Json.Schema;
using System.IO;
using System.Reflection;
using TerrificNet.Test.Common;
using Xunit;

namespace TerrificNet.ViewEngine.Schema.Test
{
    
    public class SchemaComparerTest
    {
        /// <summary>
        /// The more specific property attributes should be taken from the base set
        /// </summary>
        [Fact]
        public void TestMoreSpecific()
        {
            var comparer = new SchemaComparer();
            var resultSchema = comparer.Apply(GetSchema(Path.Combine(PathUtility.GetDirectory(), "Mocks/Comparisions/moreSpecific.json")),
                GetSchema(Path.Combine(PathUtility.GetDirectory(), "Mocks/Comparisions/moreSpecific_base.json")),
                new SchemaComparisionReport());

            Assert.NotNull(resultSchema);
            Assert.Equal("TestModel", resultSchema.Title);
            SchemaAssertions.AssertSingleProperty(resultSchema, "name", JSchemaType.Integer);
        }

        /// <summary>
        /// The base schema has an invalid type for a property that can't be converted to the one in the schema
        /// </summary>
        [Fact]
        public void TestInvalidTypeChange()
        {
            var comparer = new SchemaComparer();
            var report = new SchemaComparisionReport();
            var resultSchema = comparer.Apply(GetSchema(Path.Combine(PathUtility.GetDirectory(), "Mocks/Comparisions/invalidTypeChangeInBase.json")),
                GetSchema(Path.Combine(PathUtility.GetDirectory(), "Mocks/Comparisions/invalidTypeChangeInBase_base.json")),
                report);

            var failures = report.GetFailures().ToList();
            Assert.Equal(1, failures.Count);
            Assert.IsType(typeof(TypeChangeFailure), failures[0]);
            Assert.Equal("name", ((TypeChangeFailure)failures[0]).PropertyName);
        }

        /// <summary>
        /// An info schould be available on missing a property in the base schema.
        /// </summary>
        [Fact]
        public void TestInfoOnMissingPropertyInBaseSchema()
        {
            var comparer = new SchemaComparer();
            var report = new SchemaComparisionReport();
            var resultSchema = comparer.Apply(GetSchema(Path.Combine(PathUtility.GetDirectory(), "Mocks/Comparisions/infoOnMissingPropertyInBaseSchema.json")),
                GetSchema(Path.Combine(PathUtility.GetDirectory(), "Mocks/Comparisions/infoOnMissingPropertyInBaseSchema_base.json")),
                report);

            var infos = report.GetInfos().ToList();
            Assert.Equal(1, infos.Count);
            Assert.IsType(typeof(MissingPropertyInfo), infos[0]);
            Assert.Equal("missingPropertyInBase", ((MissingPropertyInfo)infos[0]).PropertyName);
        }

        /// <summary>
        /// Any property that is defined in the base schema should be in the result schema.
        /// </summary>
        [Fact]
        public void TestAnyPropertyFromBaseSchemaShouldBeTaken()
        {
            var comparer = new SchemaComparer();
            var report = new SchemaComparisionReport();
            var resultSchema = comparer.Apply(GetSchema(Path.Combine(PathUtility.GetDirectory(), "Mocks/Comparisions/anyPropertyInMoreSpecific.json")),
                GetSchema(Path.Combine(PathUtility.GetDirectory(), "Mocks/Comparisions/anyPropertyInMoreSpecific_base.json")),
                report);

            Assert.NotNull(resultSchema);
            Assert.Equal("val1", resultSchema.Properties["name"].Format);
            //Assert.Equal("TestModel", resultSchema.Properties["prop2"], "val2");
        }

        private static JSchema GetSchema(string path)
        {
            string content;
            using (var reader = new StreamReader(path))
            {
                content = reader.ReadToEnd();
            }

            return JSchema.Parse(content);
        }
    }
}
