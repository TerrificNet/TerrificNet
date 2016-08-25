using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Environment;

namespace TerrificNet.Controllers
{
    public class ProjectItemController : AdministrationTemplateControllerBase
    {
        public ProjectItemController(TerrificNetApplication[] applications) : base(applications)
        {
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Index(string id, string projectKind, string app)
        {
            var project = this.ResolveForApp<Project>(string.Empty);
            var pId = new ProjectItemIdentifier(id, projectKind);

            ProjectItem item;
            if (!project.TryGetItemById(pId, out item))
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            var message = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(await item.OpenRead().ConfigureAwait(false)) };
            message.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            return message;
        }
    }
}
