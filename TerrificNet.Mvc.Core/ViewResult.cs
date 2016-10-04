using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit.Compiler;
using Microsoft.Extensions.DependencyInjection;

namespace TerrificNet.Mvc.Core
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
			var compilerService = context.HttpContext.RequestServices.GetRequiredService<CompilerService>();

			var compiler = await compilerService.CreateCompiler(_path);
			var runnable = compiler.Compile(new DynamicDataBinder(), EmitterFactories.VTree);

			using (var writer = new StreamWriter(context.HttpContext.Response.Body))
			{
				writer.Write((string) runnable.Execute(_model, null).ToString());
			}
		}
	}
}