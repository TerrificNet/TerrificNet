using System.IO;
using Microsoft.Extensions.DependencyInjection;
using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Mvc.Core
{
	public static class TerrificNetServiceCollectionExtensions
	{
		public static void AddTerrificNet(this IServiceCollection services)
		{
			services.AddSingleton<IViewDiscovery>(new ViewDiscovery(Directory.GetCurrentDirectory()));
			services.AddSingleton(s => CompilerExtensions.Default
				.AddHelperBinder(new SimpleHelperBinder())
				.AddTagHelper(new MixinTagHelper(s.GetRequiredService<CompilerService>(), s.GetRequiredService<IViewDiscovery>())));

			services.AddSingleton(s => new CompilerService(s.GetRequiredService<CompilerExtensions>));
		}
	}
}