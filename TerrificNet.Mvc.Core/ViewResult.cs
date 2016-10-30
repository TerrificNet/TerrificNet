using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit.Compiler;
using Microsoft.Extensions.DependencyInjection;
using TerrificNet.Thtml.Formatting;
using TerrificNet.Thtml.Formatting.Text;
using TerrificNet.Thtml.Rendering;

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
			var builder = context.HttpContext.RequestServices.GetRequiredService<IOutputExpressionBuilderFactory>();

			var runnable = await CreateAsync(builder, context);
			await Render(context, runnable);
		}

		public async Task ExecuteChildResultAsync(MvcRenderingContext context)
		{
			var builder = context.ActionContext.HttpContext.RequestServices.GetRequiredService<IOutputExpressionBuilderFactory>();

			var runnable = await CreateAsync(builder, context.ActionContext);
			await runnable.Execute(_model, context);
		}

		private async Task<IAsyncViewTemplate> CreateAsync(IOutputExpressionBuilderFactory emitterFactory, ActionContext actionContext)
		{
			var compiler = await GetCompiler(actionContext);
			return compiler.CompileForAsync(GetDataBinder(), emitterFactory);
		}

		private IDataBinder GetDataBinder()
		{
			return TypeDataBinder.BinderFromObject(_model);
		}

		private async Task<ThtmlDocumentCompiler> GetCompiler(ActionContext actionContext)
		{
			var compilerService = actionContext.HttpContext.RequestServices.GetRequiredService<CompilerService>();
			var viewDiscovery = actionContext.HttpContext.RequestServices.GetRequiredService<IViewDiscovery>();

			var viewPath = viewDiscovery.FindView(_viewName);

			return await compilerService.CreateCompiler(viewPath);
		}

		private async Task Render(ActionContext context, IAsyncViewTemplate runnable)
		{
			using (var writer = new StreamWriter(context.HttpContext.Response.Body))
			{
				await runnable.Execute(_model, new MvcRenderingContext(new TextWriterOutputBuilder(writer), context));
			}
		}
	}
}