using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.Rendering;
using System.Linq;

namespace TerrificNet.Sample.Controllers
{
	public class HomePageController : ControllerBase
	{
		[HttpGet]
		public IActionResult Index()
		{
			return new ViewResult("asdf", new { title = "Start", body = "asdf", list = new[] { "s1", "s2" } });
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
			var compiler = await Compile(path);
			var runnable = compiler.Compile(new DynamicDataBinder(), new MixinEmitterFactory<IVTreeRenderer>(EmitterFactories.VTree));

			using (var writer = new StreamWriter(context.HttpContext.Response.Body))
			{
				//runnable.Execute(writer, _model, null);
				writer.Write(runnable.Execute(_model, null).ToString());
			}
		}

		public static async Task<ThtmlDocumentCompiler> Compile(string path)
		{
			var document = await Parse(path);
			var helperBinder = new SimpleHelperBinder();

			var compiler = new ThtmlDocumentCompiler(document, helperBinder);
			return compiler;
		}

		private static async Task<Document> Parse(string path)
		{
			var lexer = new Lexer();

			var parser = new Parser(new HandlebarsParser());
			string text;
			using (var reader = new StreamReader(new FileStream(path, FileMode.Open)))
			{
				text = await reader.ReadToEndAsync();
			}

			var document = parser.Parse(lexer.Tokenize(text));
			return document;
		}

		private class MixinEmitterFactory<T> : IEmitterFactory<T>
		{
			private readonly IEmitterFactory<T> _adaptee;

			public MixinEmitterFactory(IEmitterFactory<T> adaptee)
			{
				_adaptee = adaptee;
			}

			public IEmitter<T> Create()
			{
				return new AdapterEmitter<T>(_adaptee.Create());
			}
		}

		private class AdapterEmitter<T> : IEmitter<T>
		{
			private readonly IEmitter<T> _adaptee;

			public AdapterEmitter(IEmitter<T> adaptee)
			{
				_adaptee = adaptee;
				this.OutputExpressionEmitter = new MixinOutputEmitter(adaptee.OutputExpressionEmitter);
			}

			public IOutputExpressionEmitter OutputExpressionEmitter { get; }

			public T WrapResult(CompilerResult result)
			{
				return _adaptee.WrapResult(result);
			}
		}

		private class MixinOutputEmitter : IOutputExpressionEmitter
		{
			private readonly IOutputExpressionEmitter _outputExpressionEmitter;

			public MixinOutputEmitter(IOutputExpressionEmitter outputExpressionEmitter)
			{
				_outputExpressionEmitter = outputExpressionEmitter;
			}

			public Expression HandleAttributeContent(ConstantAttributeContent attributeContent)
			{
				return _outputExpressionEmitter.HandleAttributeContent(attributeContent);
			}

			public Expression HandleElement(Element element, INodeVisitor<Expression> visitor)
			{
				if (element.TagName.StartsWith("mixin:"))
				{
					var partialName = element.TagName.Remove(0, "mixin:".Length).Replace("-", "");
					var document = ViewResult.Parse($@"D:\projects\TerrificNet\TerrificNet.Sample\components\modules\{partialName}\{partialName}.html").Result;

					var emitVisitor = visitor as EmitExpressionVisitor;

					var subContract = element.Accept(new FillDictionaryOutputVisitor(emitVisitor._dataScopeContract));
					var visitor2 = new EmitExpressionVisitor(subContract, null, _outputExpressionEmitter);

					return visitor2.Visit(document);
				}

				return _outputExpressionEmitter.HandleElement(element, visitor);
			}

			public IEnumerable<Expression> HandleAttributeNode(AttributeNode attributeNode, Expression valueEmitter)
			{
				return _outputExpressionEmitter.HandleAttributeNode(attributeNode, valueEmitter);
			}

			public Expression HandleCall(Expression callExpression)
			{
				return _outputExpressionEmitter.HandleCall(callExpression);
			}

			public Expression HandleTextNode(TextNode textNode)
			{
				return _outputExpressionEmitter.HandleTextNode(textNode);
			}

			public Expression HandleDocument(List<Expression> expressions)
			{
				return _outputExpressionEmitter.HandleDocument(expressions);
			}

			public Expression HandleCompositeAttribute(CompositeAttributeContent compositeAttributeContent, INodeVisitor<Expression> visitor)
			{
				return _outputExpressionEmitter.HandleCompositeAttribute(compositeAttributeContent, visitor);
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

				public override Expression CreateExpression(HelperParameters helperParameters, Expression children)
				{
					return helperParameters.OutputExpressionEmitter.HandleTextNode(new TextNode(_helper));
				}
			}
		}
	}
}
