using System.Collections.Generic;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Test.Asserts;
using TerrificNet.Thtml.Test.Stubs;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class LexerTest
	{
		[Theory]
		[MemberData(nameof(TestData))]
		public void TestLexerTokenization(string input, IEnumerable<Token> expectedResult)
		{
			var lexer = new Lexer();
			var result = lexer.Tokenize(input);

			Assert.NotNull(result);
			Assert.Equal(expectedResult, result, new TokenEqualityComparer());
		}

		public static IEnumerable<object[]> TestData
		{
			get
			{
				yield return new object[]
				{
						  "",
						  TokenFactory.DocumentList()
				};
				yield return new object[]
				{
						  " ",
						  TokenFactory.DocumentList(
								i => TokenFactory.Whitespace(" ", i))
				};
				yield return new object[]
				{
						  "   ",
						  TokenFactory.DocumentList(
								i => TokenFactory.Whitespace("   ", i))
				};
				yield return new object[]
				{
						  "<html>",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.ElementStart,
									 TokenFactory.BracketOpen,
									 a => TokenFactory.Name("html", a),
									 TokenFactory.BracketClose))
				};
				yield return new object[]
				{
						  "<html >",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.ElementStart,
									 TokenFactory.BracketOpen,
									 a => TokenFactory.Name("html", a),
									 TokenFactory.Whitespace,
									 TokenFactory.BracketClose))
				};
				yield return new object[]
				{
						  "<html attribute>",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.ElementStart,
									 TokenFactory.BracketOpen,
									 a => TokenFactory.Name("html", a),
									 TokenFactory.Whitespace,
									 a => TokenFactory.Composite(a,
										  TokenCategory.Attribute,
										  b => TokenFactory.Name("attribute", b)),
									 TokenFactory.BracketClose))
				};

				yield return new object[]
				{
						  "<html attribute=\"hallo\">",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.ElementStart,
									 TokenFactory.BracketOpen,
									 a => TokenFactory.Name("html", a),
									 TokenFactory.Whitespace,
									 a => TokenFactory.Composite(a,
										  TokenCategory.Attribute,
										  b => TokenFactory.Name("attribute", b),
										  TokenFactory.Equal,
										  TokenFactory.Quote,
										  b => TokenFactory.AttributeContent("hallo", b),
										  TokenFactory.Quote),
									 TokenFactory.BracketClose))
				};
				yield return new object[]
				{
						  "<html attribute=\"\">",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.ElementStart,
									 TokenFactory.BracketOpen,
									 a => TokenFactory.Name("html", a),
									 TokenFactory.Whitespace,
									 a => TokenFactory.Composite(a,
										  TokenCategory.Attribute,
										  b => TokenFactory.Name("attribute", b),
										  TokenFactory.Equal,
										  TokenFactory.Quote,
										  TokenFactory.Quote),
									 TokenFactory.BracketClose))
				};
				yield return new object[]
				{
						  "<html attribute=\"hallo\" att2=\"val2\">",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.ElementStart,
									 TokenFactory.BracketOpen,
									 a => TokenFactory.Name("html", a),
									 TokenFactory.Whitespace,
									 a => TokenFactory.AttributeWithContent(a, "attribute", "hallo"),
									 TokenFactory.Whitespace,
									 a => TokenFactory.AttributeWithContent(a, "att2", "val2"),
									 TokenFactory.BracketClose))
				};
				yield return new object[]
				{
						  "<html ><h1>",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.ElementStart,
									 TokenFactory.BracketOpen,
									 a => TokenFactory.Name("html", a),
									 TokenFactory.Whitespace,
									 TokenFactory.BracketClose),
								i => TokenFactory.Composite(i,
									 TokenCategory.ElementStart,
									 TokenFactory.BracketOpen,
									 a => TokenFactory.Name("h1", a),
									 TokenFactory.BracketClose))
				};
				yield return new object[]
				{
						  "</h1>",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.ElementEnd,
									 TokenFactory.BracketOpen,
									 TokenFactory.Slash,
									 a => TokenFactory.Name("h1", a),
									 TokenFactory.BracketClose))
				};
				yield return new object[]
				{
						  "</h1  >",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.ElementEnd,
									 TokenFactory.BracketOpen,
									 TokenFactory.Slash,
									 a => TokenFactory.Name("h1", a),
									 a => TokenFactory.Whitespace("  ", a),
									 TokenFactory.BracketClose))
				};
				yield return new object[]
				{
						  "<h1>content</h1>",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.ElementStart,
									 TokenFactory.BracketOpen,
									 a => TokenFactory.Name("h1", a),
									 TokenFactory.BracketClose),
								i => TokenFactory.Content("content", i),
								i => TokenFactory.Composite(i,
									 TokenCategory.ElementEnd,
									 TokenFactory.BracketOpen,
									 TokenFactory.Slash,
									 a => TokenFactory.Name("h1", a),
									 TokenFactory.BracketClose))
				};
				yield return new object[]
				{
						  "<h1 attr />",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.EmptyElement,
									 TokenFactory.BracketOpen,
									 a => TokenFactory.Name("h1", a),
									 TokenFactory.Whitespace,
									 a => TokenFactory.Composite(a,
										  TokenCategory.Attribute,
										  b => TokenFactory.Name("attr", b),
										  TokenFactory.Whitespace),
									 TokenFactory.Slash,
									 TokenFactory.BracketClose))
				};

				yield return new object[]
				{
						  "<div> <h1 attr /> </div>",
						  TokenFactory.DocumentList(
								i => TokenFactory.ElementStart("div", i),
								i => TokenFactory.Content(" ", i),
								i => TokenFactory.Composite(i,
									 TokenCategory.EmptyElement,
									 TokenFactory.BracketOpen,
									 a => TokenFactory.Name("h1", a),
									 TokenFactory.Whitespace,
									 a => TokenFactory.Composite(a,
										  TokenCategory.Attribute,
										  b => TokenFactory.Name("attr", b),
										  TokenFactory.Whitespace),
									 TokenFactory.Slash,
									 TokenFactory.BracketClose),
								i => TokenFactory.Content(" ", i),
								i => TokenFactory.ElementEnd("div", i))
				};

				yield return new object[]
				{
						  "<!-- com -->",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i, TokenCategory.Comment,
									 TokenFactory.BracketOpen,
									 TokenFactory.CommentStart,
									 TokenFactory.Dash,
									 TokenFactory.Dash,
									 a => TokenFactory.CommentContent(a, " com "),
									 TokenFactory.Dash,
									 TokenFactory.Dash,
									 TokenFactory.BracketClose))
				};

				// Handlebars
				yield return new object[]
				{
						  "{{!-- comment --}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i, TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.Comment,
										  TokenFactory.CommentStart,
										  TokenFactory.Dash,
										  TokenFactory.Dash,
										  b => TokenFactory.CommentContent(b, " comment "),
										  TokenFactory.Dash,
										  TokenFactory.Dash),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};
				yield return new object[]
				{
						  "{{name}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluate, b => TokenFactory.Expression(b, "name")),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};
				yield return new object[]
				{
						  "{{  name }}",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluate,
										  b => TokenFactory.Whitespace("  ", b),
										  b => TokenFactory.Expression(b,
												c => TokenFactory.Name("name", c),
												TokenFactory.Whitespace)),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};
				yield return new object[]
				{
						  "{ {  name }}",
						  TokenFactory.DocumentList(
								i => TokenFactory.Content("{ {  name }}", i))
				};
				yield return new object[]
				{
						  "{{name. value}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluate,
										  b => TokenFactory.Expression(b,
												c => TokenFactory.Name("name", c),
										  TokenFactory.Dot,
										  TokenFactory.Whitespace,
										  c => TokenFactory.Expression(c, "value")
									 )),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};
				yield return new object[]
				{
						  "<h1>Ich bin {{name}}.</h1>{{end}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.ElementStart("h1", i),
								i => TokenFactory.Content("Ich bin ", i),
								i => TokenFactory.HandlebarsSimple(i, "name"),
								i => TokenFactory.Content(".", i),
								i => TokenFactory.ElementEnd("h1", i),
								i => TokenFactory.HandlebarsSimple(i, "end"))
				};
				yield return new object[]
				{
						  "{{#group}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsBlockStart,
										  TokenFactory.Hash,
										  b => TokenFactory.Name("group", b)),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};
				yield return new object[]
				{
					"{{@body}}",
					TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluate,
										  TokenFactory.Ad,
										  b => TokenFactory.Expression(b, "body")),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};
				yield return new object[]
				{
						  "{{e1}}{{e2}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.HandlebarsSimple(i, "e1"),
								i => TokenFactory.HandlebarsSimple(i, "e2"))
				};
				yield return new object[]
				{
						  "{{/if}}",
						  TokenFactory.DocumentList(
								TokenFactory.IfEndExpression)
				};

				yield return new object[]
				{
						  "{{/test-val}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsBlockEnd,
										  TokenFactory.Slash,
										  b => TokenFactory.Name("test-val", b)),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};

				yield return new object[]
				{
						  "{{#if expression}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.IfStartExpression("expression", i))
				};
				yield return new object[]
				{
						  "<h1 attr=\"{{test}}\">",
						  TokenFactory.DocumentList(
								i => TokenFactory.ElementStart("h1", i,
									 a => TokenFactory.AttributeWithContentExtended(a, "attr",
										  b => TokenFactory.HandlebarsSimple(b, "test"))))
				};
				yield return new object[]
				{
						  "<h1 attr=\"before{{test}}after\">",
						  TokenFactory.DocumentList(
								i => TokenFactory.ElementStart("h1", i,
									 a => TokenFactory.AttributeWithContentExtended(a, "attr",
										  b => TokenFactory.AttributeContent("before", b),
										  b => TokenFactory.HandlebarsSimple(b, "test"),
										  b => TokenFactory.AttributeContent("after", b))))
				};
				yield return new object[]
				{
						  "<h1 {{#if true}}attr=\"val\"{{/if}}>",
						  TokenFactory.DocumentList(
								i => TokenFactory.ElementStart("h1", i,
									 a => TokenFactory.IfStartExpression("true", a),
									 a => TokenFactory.AttributeWithContent(a, "attr", "val"),
									 TokenFactory.IfEndExpression))
				};

				yield return new object[]
				{
						  "<h1 attr=\"val {{#if true}}so{{/if}}\">",
						  TokenFactory.DocumentList(
								i => TokenFactory.ElementStart("h1", i,
									 a => TokenFactory.AttributeWithContentExtended(a, "attr",
										  b => TokenFactory.AttributeContent("val ", b),
										  b => TokenFactory.IfStartExpression("true", b),
										  b => TokenFactory.AttributeContent("so", b),
										  TokenFactory.IfEndExpression)))
				};

				yield return new object[]
				{
						  "<h1 attr=\"val {{#if true}}so-{{do}}{{/if}}\">",
						  TokenFactory.DocumentList(
								i => TokenFactory.ElementStart("h1", i,
									 a => TokenFactory.AttributeWithContentExtended(a, "attr",
										  b => TokenFactory.AttributeContent("val ", b),
										  b => TokenFactory.IfStartExpression("true", b),
										  b => TokenFactory.AttributeContent("so-", b),
										  b => TokenFactory.HandlebarsSimple(b, "do"),
										  TokenFactory.IfEndExpression)))
				};
				yield return new object[]
				{
						  "{{{test}}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluateInHtml,
										  TokenFactory.HandlebarsStart,
										  b => TokenFactory.Expression(b, "test"),
										  TokenFactory.HandlebarsEnd),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};
				yield return new object[]
				{
						  "{{#name param1=1/2}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsBlockStart,
										  TokenFactory.Hash,
										  b => TokenFactory.Name("name", b),
										  TokenFactory.Whitespace,
										  b => TokenFactory.Composite(b, TokenCategory.HandlebarsAttribute,
												c => TokenFactory.Name("param1", c),
												TokenFactory.Equal,
												c => TokenFactory.AttributeContent("1/2", c))),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};
				yield return new object[]
				{
						  "{{name param1=\"val\"}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluate,
										  b => TokenFactory.Composite(b, TokenCategory.HandlebarsExpression,
												c => TokenFactory.Name("name", c),
												TokenFactory.Whitespace,
												c => TokenFactory.Composite(c, TokenCategory.HandlebarsAttribute,
													 d => TokenFactory.Name("param1", d),
													 TokenFactory.Equal,
													 TokenFactory.Quote,
													 d => TokenFactory.AttributeContent("val", d),
													 TokenFactory.Quote))),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};
				yield return new object[]
				{
						  "{{name param1=\"val\" param2=\"val2\"}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluate,
										  b => TokenFactory.Composite(b, TokenCategory.HandlebarsExpression,
												c => TokenFactory.Name("name", c),
												TokenFactory.Whitespace,
												c => TokenFactory.Composite(c, TokenCategory.HandlebarsAttribute,
													 d => TokenFactory.Name("param1", d),
													 TokenFactory.Equal,
													 TokenFactory.Quote,
													 d => TokenFactory.AttributeContent("val", d),
													 TokenFactory.Quote),
												TokenFactory.Whitespace,
												c => TokenFactory.Composite(c, TokenCategory.HandlebarsAttribute,
													 d => TokenFactory.Name("param2", d),
													 TokenFactory.Equal,
													 TokenFactory.Quote,
													 d => TokenFactory.AttributeContent("val2", d),
													 TokenFactory.Quote))),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};
				yield return new object[]
				{
					"<prefix:tagName></prefix:tagName>",
					TokenFactory.DocumentList(
						i => TokenFactory.ElementStart("prefix:tagName", i),
						i => TokenFactory.ElementEnd("prefix:tagName", i))
				};
				yield return new object[]
				{
					"{{this}}",
					TokenFactory.DocumentList(
						i => TokenFactory.HandlebarsSimple(i, "this"))
				};
				yield return new object[]
				{
					"{{../hallo}}",
					TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluate,
										  b => TokenFactory.Expression(b,
												TokenFactory.Parent,
												c => TokenFactory.Expression(c, "hallo")
									 )),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};
				yield return new object[]
				{
					"{{../../hallo}}",
					TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluate,
										  b => TokenFactory.Expression(b,
												TokenFactory.Parent,
												c => TokenFactory.Expression(c,
													TokenFactory.Parent,
													d => TokenFactory.Expression(d, "hallo"))
									 )),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};

				// Support $ as valid object name
				yield return new object[]
				{
					"{{$scope}}",
					TokenFactory.DocumentList(
						i => TokenFactory.HandlebarsSimple(i, "$scope"))
				};

				yield return new object[]
				{
					"{{$scope.add}}",
						  TokenFactory.DocumentList(
								i => TokenFactory.Composite(i,
									 TokenCategory.External,
									 TokenFactory.HandlebarsStart,
									 TokenFactory.HandlebarsStart,
									 a => TokenFactory.Composite(a, TokenCategory.HandlebarsEvaluate,
										  b => TokenFactory.Expression(b,
												c => TokenFactory.Name("$scope", c),
										  TokenFactory.Dot,
										  c => TokenFactory.Expression(c, "add")
									 )),
									 TokenFactory.HandlebarsEnd,
									 TokenFactory.HandlebarsEnd))
				};
			}
		}
	}
}
