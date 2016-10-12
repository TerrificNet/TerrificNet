using System.IO;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using TerrificNet.Thtml.Emit.Compiler;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;

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
				.AddTagHelper(new ModuleTagHelper(s.GetRequiredService<IControllerFactory>(), s.GetRequiredService<IActionDescriptorCollectionProvider>(), s.GetRequiredService<IHttpContextAccessor>())));

			services.AddSingleton(s => new CompilerService(s.GetRequiredService<CompilerExtensions>));

			// TODO: remove and replace with property HttpContext calls
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
		}
	}
}