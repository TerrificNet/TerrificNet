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
			var runnable = await CreateAsync(EmitterFactories.VTree, context);
			Render(context, runnable);
		}

		private async Task<IViewTemplate<T>> CreateAsync<T>(IEmitterFactory<T> emitterFactory, ActionContext actionContext)
		{
			var compiler = await GetCompiler(actionContext);
			return compiler.Compile(GetDataBinder(), emitterFactory);
		}

		public void Execute(IEmitter emitter, object renderer, ActionContext actionContext)
		{
			var func = Create(emitter, actionContext);
			func.Execute(renderer, _model, null);
		}

		private IViewTemplate Create(IEmitter emitter, ActionContext actionContext)
		{
			var compiler = GetCompiler(actionContext).Result;
			return compiler.Compile(GetDataBinder(), emitter);
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

		private void Render(ActionContext context, IViewTemplate<IVDomBuilder> runnable)
		{
			using (var writer = new StreamWriter(context.HttpContext.Response.Body))
			{
				var builder = new VDomBuilder();
				runnable.Execute(builder, _model, null);
				var vTree = builder.ToDom();

				writer.Write(vTree.ToString());
			}
		}
	}
}