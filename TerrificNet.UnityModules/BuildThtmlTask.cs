using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TerrificNet.Environment;
using TerrificNet.Environment.Building;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.UnityModules
{
    public class BuildThtmlTask : IBuildTask
    {
        private readonly Project _project;
        public BuildQuery DependsOn => BuildQuery.SingleFromKind("template");
        public BuildOptions Options => BuildOptions.BuildOnRequest;
        public string Name => "thtml";

        public BuildThtmlTask(Project project)
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
            return new BuildTaskResult(new ProjectItemIdentifier(id, "thtml"),
                new ProjectItemContentFromAction(() => Parse(item)));
        }

        private static async Task<Stream> Parse(ProjectItem item)
        {
            using (var stream = new StreamReader(await item.OpenRead()))
            {
                var text = stream.ReadToEnd();
                var tree = new Parser(new HandlebarsParser()).Parse(new Lexer().Tokenize(text));
                var memoryStream = new MemoryStream();
                var streamWriter = new StreamWriter(memoryStream);
                new JsonSerializer().Serialize(streamWriter, tree);
                streamWriter.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);

                return memoryStream;
            }
        }
    }
}