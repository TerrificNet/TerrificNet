using System;
using System.IO;
using System.Threading.Tasks;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Mvc.Core
{
	public class CompilerService
	{
		private readonly Func<CompilerExtensions> _extensionsProvider;

		private CompilerExtensions Extensions => _extensionsProvider();

		public CompilerService(Func<CompilerExtensions> extensionsProvider)
		{
			_extensionsProvider = extensionsProvider;
		}

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