using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using TerrificNet.Environment;

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
				defaults: new { controller = "HomePage", action = "Index" });
			});

			app.UseDeveloperExceptionPage();

			app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "views")),
				RequestPath = "/static"
			});
		}
	}
}
