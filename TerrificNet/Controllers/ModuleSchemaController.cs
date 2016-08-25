using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.ViewEngine;

namespace TerrificNet.Controllers
{
    public class ModuleSchemaController : Controller
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IModuleSchemaProvider _schemaProvider;

        public ModuleSchemaController(IModuleRepository moduleRepository, IModuleSchemaProvider schemaProvider)
        {
            _moduleRepository = moduleRepository;
            _schemaProvider = schemaProvider;
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Get(string path)
        {
            var moduleDefinition = await _moduleRepository.GetModuleDefinitionByIdAsync(path).ConfigureAwait(false);
            if (moduleDefinition == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Template not found") };

            var schema = await _schemaProvider.GetSchemaFromModuleAsync(moduleDefinition).ConfigureAwait(false);
            var message = new HttpResponseMessage
            {
                Content = new StringContent(schema.ToString())
            };
            message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return message;            
        }

    }
}