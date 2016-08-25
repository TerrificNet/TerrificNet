using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace TerrificNet.Controllers
{
    public class AdministrationTemplateControllerBase : TemplateControllerBase
    {
        private readonly TerrificNetApplication[] _applications;

        protected AdministrationTemplateControllerBase(TerrificNetApplication[] applications)
        {
            _applications = applications;
        }

        protected T ResolveForApp<T>(string applicationName)
        {
            applicationName = applicationName ?? string.Empty;
            var application = _applications.First(a => a.Section == applicationName);

            return application.Container.GetRequiredService<T>();
        }
    }

   public class TerrificNetApplication
   {
      public string Section { get; set; }
      public IServiceProvider Container { get; set; }
      public string Name { get; set; }
   }
}