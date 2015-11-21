using System.IO;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Linq;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Client.test
{
    [Route("test")]
    public class TestController : Controller
    {
        [HttpPost]
        public IActionResult Get([FromQuery] string template, [FromBody] JToken obj)
        {
            var emitter = CreateEmitter(new DynamicDataBinder(), new NullHelperBinder(), template);

            var vTree = emitter.Execute(obj, null);
            return new ObjectResult(vTree);
        }

	    private static IRunnable<VTree> CreateEmitter(IDataBinder dataBinder, IHelperBinder helperBinder, string path)
        {
            string template;
            using (var reader = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                template = reader.ReadToEnd();
            }

            var lexer = new Lexer();
            var tokens = lexer.Tokenize(template);
            var parser = new Parser(new HandlebarsParser());
            var ast = parser.Parse(tokens);
            var compiler = new ThtmlDocumentCompiler(ast, helperBinder);

            var emitter = compiler.CompileForVTree(dataBinder);
            return emitter;
        }
    }
}
