using System.IO;
using Microsoft.AspNet.Mvc;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Client.test
{
    [Route("test")]
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult Get(string template)
        {
            var emitter = CreateEmitter(new DynamicDataBinder(), new NullHelperBinder(), template);

            var vTree = emitter.Execute(new ObjectDataContext(null), null);
            return new ObjectResult(vTree);
        }

        internal static IEmitterRunnable<VTree> CreateEmitter(IDataBinder dataBinder, IHelperBinder helperBinder, string path)
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
            var compiler = new VTreeEmitter();

            var emitter = compiler.Emit(ast, dataBinder, helperBinder);
            return emitter;
        }
    }
}
