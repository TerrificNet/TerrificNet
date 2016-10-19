using System.Collections.Generic;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Formatting;
using TerrificNet.Thtml.Formatting.VDom;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.Rendering;
using TerrificNet.Thtml.Test.Stubs;
using TerrificNet.Thtml.VDom;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class IntegrationTest
	{
		[Theory]
		[MemberData(nameof(TestData))]
		public void TestFullStack(string inputTemplate, object inputObject, string expectedResult)
		{
			var lexer = new Lexer();
			var tokens = lexer.Tokenize(inputTemplate);
			var parser = new Parser(new HandlebarsParser());
			var ast = parser.Parse(tokens);
			var dataBinder = TypeDataBinder.BinderFromObject(inputObject);
			var method = new ThtmlDocumentCompiler(ast, CompilerExtensions.Default).Compile(dataBinder, OutputFactories.VTree);

			var builder = new VDomBuilder();
			method.Execute(inputObject, new RenderingContext(new VDomOutputBuilder(builder)));
			var result = builder.ToDom();

			Assert.Equal(expectedResult, result.ToString());
		}

		public static IEnumerable<object[]> TestData
		{
			get
			{
				yield return new object[]
				{
						  @"<h1><ul>{{#each items}}<li>{{name}}</li>{{/each}}</ul></h1>",
						  new
						  {
								Items = new[] { new Dummy { Name = "test1" }, new Dummy { Name = "test2" } }
						  },
						  @"<h1><ul><li>test1</li><li>test2</li></ul></h1>"
				};
				yield return new object[]
				{
						  @"<div name=""{{name}}"" />",
						  new Dummy { Name = "test1" },
						  @"<div name=""test1""></div>"
				};
			}
		}
	}
}
