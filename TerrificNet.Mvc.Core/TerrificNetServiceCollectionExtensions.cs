using Microsoft.Extensions.DependencyInjection;
using TerrificNet.Thtml.Emit.Compiler;

namespace TerrificNet.Mvc.Core
{
	public static class TerrificNetServiceCollectionExtensions
	{
		public static void AddTerrificNet(this IServiceCollection services)
		{
			services.AddSingleton(s => CompilerExtensions.Default
				.AddHelperBinder(new SimpleHelperBinder())
				.AddTagHelper(new MixinTagHelper(s.GetRequiredService<CompilerService>())));

			services.AddSingleton(s => new CompilerService(s.GetRequiredService<CompilerExtensions>));
		}
	}
}