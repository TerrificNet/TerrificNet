using System.IO;
using Microsoft.Extensions.DependencyInjection;
using TerrificNet.Thtml.Emit.Compiler;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;
using TerrificNet.Thtml.Formatting;

namespace TerrificNet.Mvc.Core
{
	public static class TerrificNetServiceCollectionExtensions
	{
		public static void AddTerrificNet(this IServiceCollection services)
		{
			services.AddSingleton<IViewDiscovery>(new ViewDiscovery(Directory.GetCurrentDirectory()));
			services.AddSingleton(s => CompilerExtensions.Default
				.AddHelperBinder(new SimpleHelperBinder())
				.AddTagHelper(new MixinTagHelper(s.GetRequiredService<CompilerService>(), s.GetRequiredService<IViewDiscovery>()))
				.AddTagHelper(new ModuleTagHelper(s.GetRequiredService<IActionDescriptorCollectionProvider>())));

			services.AddSingleton(s => new CompilerService(s.GetRequiredService<CompilerExtensions>));
			services.AddTransient<IOutputExpressionBuilderFactory>(p => OutputFactories.VTree);

			// TODO: remove and replace with property HttpContext calls
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		}
	}
}