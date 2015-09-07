using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;
using TerrificNet.Environment;
using TerrificNet.Environment.Building;

namespace TerrificNet.AssetCompiler
{
    public class CompileJavascriptTask : IBuildTask
    {
        private readonly string _output;

        public CompileJavascriptTask(ProjectItemIdentifier inputItem, string output)
        {
            _output = output;
            this.DependsOn = BuildQuery.Exact(inputItem);
        }

        public BuildQuery DependsOn { get; }
        public BuildOptions Options => BuildOptions.BuildOnRequest;
        public string Name => "compiled_js";
        public IEnumerable<BuildTaskResult> Proceed(IEnumerable<ProjectItem> items)
        {
            return
                items.Select(
                    s =>
                        new BuildTaskResult(new ProjectItemIdentifier(_output, "compiled_js"),
                            new ProjectItemContentFromAction(() => Do(s))));
        }

        private static async Task<Stream> Do(ProjectItem projectItem)
        {
            string content;
            using (var reader = new StreamReader(await projectItem.OpenRead().ConfigureAwait(false)))
            {
                content = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            var minifier = new Minifier();
            content = minifier.MinifyJavaScript(content);

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(content);
            writer.Flush();

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}