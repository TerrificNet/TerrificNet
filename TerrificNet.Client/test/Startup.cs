using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;

namespace TerrificNet.Client.test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder builder)
        {
            builder.UseMvc();
        }
    }
}
