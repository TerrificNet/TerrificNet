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
		private readonly string _viewName;
		private readonly object _model;

		public ViewResult(string viewName, object model)
		{
			_viewName = viewName;
			_model = model;
		}

		public async Task ExecuteResultAsync(ActionContext context)
		{
			var compilerService = context.HttpContext.RequestServices.GetRequiredService<CompilerService>();
			var viewDiscovery = context.HttpContext.RequestServices.GetRequiredService<IViewDiscovery>();

			var viewPath = viewDiscovery.FindView(_viewName);

			var compiler = await compilerService.CreateCompiler(viewPath);
			var runnable = compiler.Compile(TypeDataBinder.BinderFromObject(_model), EmitterFactories.VTree);

			using (var writer = new StreamWriter(context.HttpContext.Response.Body))
			{
				writer.Write(runnable.Execute(_model, null).ToString());
			}
		}
	}
}