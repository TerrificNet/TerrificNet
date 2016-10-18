using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit.Compiler;
using Microsoft.Extensions.DependencyInjection;
using TerrificNet.Thtml.Rendering;
using TerrificNet.Thtml.VDom;

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
			var builder = context.HttpContext.RequestServices.GetRequiredService<IOutputExpressionBuilder>();

			var runnable = await CreateAsync(builder, context);
			Render(context, runnable);
		}

		public async Task ExecuteChildResultAsync(ActionContext context)
		{
			var builder = context.HttpContext.RequestServices.GetRequiredService<IOutputExpressionBuilder>();

			var runnable = await CreateAsync(builder, context);

			var renderer = context.HttpContext.Features.Get<VDomBuilder>();
			runnable.Execute(renderer, _model, new MvcRenderingContext(context));
		}

		private async Task<IViewTemplate> CreateAsync(IOutputExpressionBuilder emitterFactory, ActionContext actionContext)
		{
			var compiler = await GetCompiler(actionContext);
			return compiler.Compile(GetDataBinder(), emitterFactory);
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

		private void Render(ActionContext context, IViewTemplate runnable)
		{
			using (var writer = new StreamWriter(context.HttpContext.Response.Body))
			{
				var builder = new VDomBuilder();
				context.HttpContext.Features.Set(builder);
				runnable.Execute(builder, _model, new MvcRenderingContext(context));
				var vTree = builder.ToDom();

				writer.Write(vTree.ToString());
			}
		}
	}
}