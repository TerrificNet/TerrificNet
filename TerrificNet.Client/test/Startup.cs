using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Cors;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.Framework.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace TerrificNet.Client.test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
            //        .AllowAnyMethod()
            //        .AllowAnyHeader());
            //});

            services.AddMvc().AddMvcOptions(m =>
            {
                m.OutputFormatters.Clear();

                var formatter = new JsonOutputFormatter
                {
                    SerializerSettings = { ContractResolver = new CamelCasePropertyNamesContractResolver() }
                };

                m.OutputFormatters.Add(formatter);
            });
        }

        public void Configure(IApplicationBuilder builder)
        {
            builder.Use(async (context, next) =>
            {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", context.Request.Headers["Origin"]);
                    context.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "Origin, X-Requested-With, Content-Type, Accept" });
                    context.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET, POST, PUT, DELETE, OPTIONS" });
                    context.Response.Headers.Add("Access-Control-Allow-Credentials", new[] { "true" });

                if (context.Request.Method == "OPTIONS")
                    return;

                await next();
            });
            builder.UseMvc();
            //builder.UseCors("AllowAll");
        }
    }
}
