using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Environment;

namespace TerrificNet.Controllers
{
    public class ProjectController : AdministrationTemplateControllerBase
    {
        public ProjectController(TerrificNetApplication[] applications) : base(applications)
        {
        }

        [HttpGet]
        public IEnumerable<ProjectItemDto> Index(string app)
        {
            var project = this.ResolveForApp<Project>(string.Empty);
            return project.GetItems().Select(s => new ProjectItemDto { Identifier = s.Identifier.Identifier, Kind = s.Identifier.Kind, Url = $"web/project/{app}/{s.Identifier.Kind}/{s.Identifier.Identifier}" });
        }

        public class ProjectItemDto
        {
            public string Identifier { get; set; }
            public string Kind { get; set; }
            public string Url { get; set; }
        }
    }
}
