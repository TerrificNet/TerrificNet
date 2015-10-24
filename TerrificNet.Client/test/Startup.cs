using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.Framework.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace TerrificNet.Client.test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddMvcOptions(m =>
            {
                m.OutputFormatters.Clear();

                var formatter = new JsonOutputFormatter
                {
                    SerializerSettings = {ContractResolver = new CamelCasePropertyNamesContractResolver()}
                };

                m.OutputFormatters.Add(formatter);
            });
        }

        public void Configure(IApplicationBuilder builder)
        {
            builder.Use(async (context, next) => { context.Response.Headers.Add("Access-Control-Allow-Origin", "*"); await next(); });
            builder.UseMvc();
        }
    }
}
