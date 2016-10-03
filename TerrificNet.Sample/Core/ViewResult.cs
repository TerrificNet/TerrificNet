using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Sample.Core
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
			var compilerService = new CompilerService();
			var compilerExtensions = CompilerExtensions.Default
				.AddHelperBinder(new SimpleHelperBinder())
				.AddTagHelper(new MixinTagHelper(compilerService));

			compilerService.Extensions = compilerExtensions;

			var compiler = await compilerService.CreateCompiler(_path);
			var runnable = compiler.Compile(new DynamicDataBinder(), EmitterFactories.VTree);

			using (var writer = new StreamWriter(context.HttpContext.Response.Body))
			{
				writer.Write(runnable.Execute(_model, null).ToString());
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

				public override Expression CreateExpression(HelperParameters helperParameters)
				{
					return helperParameters.Visitor.Visit(new TextNode(_helper));
				}
			}
		}
	}
}