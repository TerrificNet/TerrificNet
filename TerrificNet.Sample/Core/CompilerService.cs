using System.IO;
using System.Threading.Tasks;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Sample.Core
{
	public class CompilerService
	{
		internal CompilerExtensions Extensions { get; set; }

		public async Task<ThtmlDocumentCompiler> CreateCompiler(string path)
		{
			var document = await Parse(path);

			return new ThtmlDocumentCompiler(document, Extensions);
		}

		public async Task<Document> Parse(string path)
		{
			var lexer = new Lexer();

			var parser = new Parser(new HandlebarsParser());
			string text;
			using (var reader = new StreamReader(new FileStream(path, FileMode.Open)))
			{
				text = await reader.ReadToEndAsync();
			}

			var document = parser.Parse(lexer.Tokenize(text));
			return document;
		}
	}
}