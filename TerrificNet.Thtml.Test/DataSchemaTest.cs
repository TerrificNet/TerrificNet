using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Test.Stubs;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class DataSchemaTest
	{
		[Theory]
		[MemberData("TestData")]
		public void TestDataSchemaToTypescriptDefinition(string expected, DataSchema schema)
		{
			var builder = new StringBuilder();
			using (var writer = new StringWriter(builder))
			{
				var visitor = new TypeScriptDefinitionVisitor(writer);
				schema.Accept(visitor);
			}

			var actual = builder.ToString();
			Assert.Equal(NoWhitespace(expected), NoWhitespace(actual));
		}

		private string NoWhitespace(string value)
		{
			return Regex.Replace(value, "(\\s)+", " ");
		}

		public static IEnumerable<object[]> TestData
		{
			get
			{
				var dependentNodes = new [] { SyntaxNodeStub.Node1 };	
				yield return new object[]
				{
					@"interface _global {
						property1:string;
					} ",
					new ComplexDataSchema(new[]
					{
						new DataSchemaProperty("property1", DataSchema.String, dependentNodes)
					}, false)

				};

				yield return new object[]
				{
					@"interface _global {
						property1:t1;
						property2?:t2;
					} 
					interface t2 {
						property4:string;
					}					
					interface t1 {
						property2:string;
						property3:boolean;
					} ",
					new ComplexDataSchema(new[]
					{
						new DataSchemaProperty("property1", 
							new ComplexDataSchema(new []
							{
								new DataSchemaProperty("property2", DataSchema.String, dependentNodes),
								new DataSchemaProperty("property3", DataSchema.Boolean, dependentNodes)
							}, false), 
							dependentNodes),
						new DataSchemaProperty("property2",
							new ComplexDataSchema(new []
							{
								new DataSchemaProperty("property4", DataSchema.String, dependentNodes)
							}, true),
							dependentNodes)

					}, false)

				};

				yield return new object[]
				{
					@"interface _global extends Array<string> {
						property1:string;
					} ",
					new IterableDataSchema(DataSchema.String, new[]
					{
						new DataSchemaProperty("property1", DataSchema.String, dependentNodes)
					}, false)

				};

				yield return new object[]
				{
					@"interface _global extends Array<t1> {
						property1:string;
					}
					interface t1 {
						property2:boolean;
					} ",
					new IterableDataSchema(new ComplexDataSchema(
						new []
						{
							new DataSchemaProperty("property2", DataSchema.Boolean, Enumerable.Empty<SyntaxNode>())
						}, false), new[]
					{
						new DataSchemaProperty("property1", DataSchema.String, dependentNodes)
					}, false)

				};

				yield return new object[]
				{
					@"interface _global extends Array<t1> {
						property1:string;
					}
					interface t1 {
						property2:t2;
					}
					interface t2 extends Array<t3> {
					}
					interface t3 {
						property3:string;
					} ",
					new IterableDataSchema(new ComplexDataSchema(
						new []
						{
							new DataSchemaProperty("property2",
								new IterableDataSchema(
									new ComplexDataSchema(new []
									{
										new DataSchemaProperty("property3", DataSchema.String, Enumerable.Empty<SyntaxNode>())
									}, false), false), Enumerable.Empty<SyntaxNode>())
						}, false), new[]
					{
						new DataSchemaProperty("property1", DataSchema.String, dependentNodes)
					}, false)

				};
			}
		}
	}
}
