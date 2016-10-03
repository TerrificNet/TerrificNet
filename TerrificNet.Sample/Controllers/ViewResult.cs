using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Sample.Controllers
{
	public class ViewResult : IActionResult
	{
		private readonly object _model;
		private readonly string _path;

		public ViewResult(string viewName, object model)
		{
			_model = model;
			_path = @"D:\projects\TerrificNet\TerrificNet.Sample\views\_layouts\_layout.html";
		}

		public async Task ExecuteResultAsync(ActionContext context)
		{
			var compiler = await Compile(_path);
			var runnable = compiler.Compile(new DynamicDataBinder(), EmitterFactories.VTree);

			using (var writer = new StreamWriter(context.HttpContext.Response.Body))
			{
				writer.Write(runnable.Execute(_model, null).ToString());
			}
		}

		public static async Task<ThtmlDocumentCompiler> Compile(string path)
		{
			var document = await Parse(path);
			var helperBinder = new SimpleHelperBinder();

			var compiler = new ThtmlDocumentCompiler(document, CompilerExtensions.Default.AddHelperBinder(helperBinder).AddTagHelper(new MixinTagHelper()));
			return compiler;
		}

		public static async Task<Document> Parse(string path)
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

				public override Expression CreateExpression(HelperParameters helperParameters)
				{
					return helperParameters.CompilerExtensions.OutputEmitter.HandleTextNode(new TextNode(_helper));
				}
			}
		}

		public class HelperBinderExtension : IHelperBinder
		{
			private readonly Expression _expression;

			public HelperBinderExtension(Expression expression)
			{
				_expression = expression;
			}

			public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
			{
				if ("body".Equals(helper, StringComparison.OrdinalIgnoreCase))
				{
					return HelperBinderResult.Create(param => _expression);
				}

				return null;
			}
		}
	}

	public class MixinTagHelper : ITagHelper
	{
		public HelperBinderResult FindByName(Element element)
		{
			if (element.TagName.StartsWith("mixin:"))
			{
				var partialName = element.TagName.Remove(0, "mixin:".Length).Replace("-", "");
				var document = ViewResult.Parse($@"D:\projects\TerrificNet\TerrificNet.Sample\components\modules\{partialName}\{partialName}.html").Result;

				return new MixinHelperBinderResult(document, element);
			}

			return null;
		}

		private class MixinHelperBinderResult : HelperBinderResult
		{
			private readonly Element _element;
			private readonly Document _document;

			public MixinHelperBinderResult(Document document, Element element)
			{
				_document = document;
				_element = element;
			}

			public override Expression CreateExpression(HelperParameters helperParameters)
			{
				var children2 = helperParameters.CompilerExtensions.OutputEmitter.HandleElementList(_element.ChildNodes.Select(i => i.Accept(helperParameters.Visitor)).ToList());

				var subContract = _element.Accept(new FillDictionaryOutputVisitor(helperParameters.ScopeContract));
				var visitor2 = new EmitExpressionVisitor(subContract, CompilerExtensions.Default.AddHelperBinder(new AggregatedHelperBinder(new ViewResult.HelperBinderExtension(children2))).WithEmitter(helperParameters.CompilerExtensions.OutputEmitter));

				return visitor2.Visit(_document);
			}
		}
	}
}