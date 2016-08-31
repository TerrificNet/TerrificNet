using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Sample.Controllers
{
    public class HomePageController : ControllerBase
	{
		[HttpGet]
	    public IActionResult Index()
		{
			return new ViewResult("asdf", new { title = "Start" });
		    //return View("Gugus", new {});
	    }
    }

	public class ViewResult : IActionResult
	{
		private readonly object _model;

		public ViewResult(string viewName, object model)
		{
			_model = model;
		}

		public async Task ExecuteResultAsync(ActionContext context)
		{
			string path = @"D:\projects\TerrificNet\TerrificNet.Sample\views\_layouts\_layout.html";
			//var viewEngine = context.HttpContext.RequestServices.GetRequiredService<IViewEngine>();
			var lexer = new Lexer();

			var parser = new Parser(new HandlebarsParser());
			string text;
			using (var reader = new StreamReader(new FileStream(path, FileMode.Open)))
			{
				text = await reader.ReadToEndAsync();
			}

			var document = parser.Parse(lexer.Tokenize(text));
			var helperBinder = new SimpleHelperBinder();

			var compiler = new ThtmlDocumentCompiler(document, helperBinder);
			var runnable = compiler.Compile(new DynamicDataBinder(), EmitterFactories.Stream);

			using (var writer = new StreamWriter(context.HttpContext.Response.Body))
			{
				runnable.Execute(writer, _model, null);
			}
		}

		private class SimpleHelperBinder : IHelperBinder
		{
			public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
			{
				return new SimpleHelperBinderResult(helper);
			}

			private class SimpleHelperBinderResult : HelperBinderResult
			{
				private readonly string _helper;

				public SimpleHelperBinderResult(string helper)
				{
					_helper = helper;
				}

				public override Expression CreateEmitter(HelperParameters helperParameters, Expression children)
				{
					return helperParameters.OutputExpressionEmitter.HandleTextNode(new TextNode(_helper));
				}
			}
		}
	}
}
