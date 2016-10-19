using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Formatting;
using TerrificNet.Thtml.Formatting.IncrementalDom;
using TerrificNet.Thtml.Formatting.VDom;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.Rendering;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Client.test
{
	[Route("test")]
	public class TestController : Controller
	{
		[HttpPost]
		public IActionResult Get([FromQuery] string template, [FromBody] JToken obj)
		{
			var emitter = CreateEmitter(new DynamicDataBinder(), new NullHelperBinder(), template);

			var builder = new VDomBuilder();
			emitter.Execute(obj, new RenderingContext(new VDomOutputBuilder(builder)));
			var vTree = builder.ToDom();

			return new ObjectResult(vTree);
		}

		[HttpPost("incremental")]
		public string GetIncrementalDom([FromQuery] string template, [FromBody] JToken obj)
		{
			var compiler = CreateCompiler(new NullHelperBinder(), template);
			var emitter = compiler.Compile(new DynamicDataBinder(), OutputFactories.IncrementalDomScript);

			var builder = new StringBuilder();

			var mapping = new JavascriptMethodMapping
			{
				ElementClose = "c",
				ElementOpen = "o",
				ElementVoid = "v",
				ElementOpenEnd = "e",
				ElementOpenStart = "s",
				Text = "t",
				Attr = "a"
			};

			using (var renderer = new JavascriptIncrementalDomRenderer(new StringWriter(builder), mapping))
			{
				emitter.Execute(obj, new RenderingContext(new IncrementalDomOutput(renderer)));
			}

			return builder.ToString();
		}

		private static IViewTemplate CreateEmitter(IDataBinder dataBinder, IHelperBinder helperBinder, string path)
		{
			var compiler = CreateCompiler(helperBinder, path);
			return compiler.Compile(dataBinder, OutputFactories.VTree);
		}

		private static ThtmlDocumentCompiler CreateCompiler(IHelperBinder helperBinder, string path)
		{
			string template;
			using (var reader = new StreamReader(new FileStream(path, FileMode.Open)))
			{
				template = reader.ReadToEnd();
			}

			var lexer = new Lexer();
			var tokens = lexer.Tokenize(template);
			var parser = new Parser(new HandlebarsParser());
			var ast = parser.Parse(tokens);
			var compiler = new ThtmlDocumentCompiler(ast, CompilerExtensions.Default.AddHelperBinder(helperBinder));
			return compiler;
		}
	}
}
