using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TerrificNet.Environment;
using TerrificNet.Environment.Building;

namespace TerrificNet.AssetCompiler
{
    public class BundleTask : IBuildTask
    {
        private readonly ProjectItemIdentifier _outputItemId;

        public BundleTask(string projectItemKind, ProjectItemIdentifier outputItemId)
        {
            this.DependsOn = BuildQuery.AllFromKind(projectItemKind);
            _outputItemId = outputItemId;
        }

        public BuildQuery DependsOn { get; }
        public BuildOptions Options => BuildOptions.BuildOnRequest;
        public string Name => _outputItemId.Kind;
        public IEnumerable<BuildTaskResult> Proceed(IEnumerable<ProjectItem> items)
        {
            yield return
                new BuildTaskResult(
                    _outputItemId,
                    new BundleProjectContent(items.ToList()));
        }

        private class BundleProjectContent : IProjectItemContent
        {
            private readonly IList<ProjectItem> _items;

            public BundleProjectContent(IList<ProjectItem> items)
            {
                _items = items;
            }

            public async Task<Stream> ReadAsync()
            {
                var memoryStream = new MemoryStream();
                var sb = new StreamWriter(memoryStream);
                foreach (var item in _items)
                {
                    using (var reader = new StreamReader(item.OpenRead()))
                    {
                        sb.Write(await reader.ReadToEndAsync());
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
        }
    }
}