using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace TerrificNet.Sample
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvcCore();
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseMvc(rb =>
			{
				rb.MapRoute(
				name: "default",
				template: "{controller}/{action}/{id?}",
				defaults: new { controller = "Home", action = "Index" });
			});
			app.UseStaticFiles(new StaticFileOptions
			{
			   FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "views")),
            RequestPath = "/views"
			});
		}
	}
}
