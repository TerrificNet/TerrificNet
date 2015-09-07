using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TerrificNet.Environment;
using TerrificNet.Environment.Building;

namespace TerrificNet.UnityModules
{
    public class BuildViewTask : IBuildTask
    {
        private readonly Project _project;
        public BuildQuery DependsOn => BuildQuery.SingleFromKind("view");
        public BuildOptions Options => BuildOptions.BuildOnRequest;
        public string Name => "generated_views";

        public BuildViewTask(Project project)
        {
            _project = project;
        }

        public IEnumerable<BuildTaskResult> Proceed(IEnumerable<ProjectItem> items)
        {
            return items.Select(BuildTaskResult);
        }

        private static BuildTaskResult BuildTaskResult(ProjectItem item)
        {
            var id = item.Identifier.Identifier.Replace(".html.json", "");
            return new BuildTaskResult(new ProjectItemIdentifier(id, "generated_view"), 
                new ProjectItemContentFromAction(item.OpenRead));
        }
    }
}