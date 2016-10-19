using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TerrificNet.Thtml.Formatting;
using TerrificNet.Thtml.Formatting.Text;
using Xunit;

namespace TerrificNet.Thtml.Test.Formatting
{
	public class TextWriterOutputBuilderTest
	{
		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void TextWriterOutputBuilder_ElementOpenStart_OpenBracketTagName(Func<TextWriter, IOutputBuilder> builderFactory)
		{
			const string tagName = "tagName";

			var stringBuilder = new StringBuilder();
			using (var writer = new StringWriter(stringBuilder))
			{
				var underTest = builderFactory(writer);
				underTest.ElementOpenStart(tagName, null);
			}

			Assert.Equal($"<{tagName}", stringBuilder.ToString());
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void TextWriterOutputBuilder_ElementOpenStartWithStaticProperies_OpenBracketTagNameProperties(Func<TextWriter, IOutputBuilder> builderFactory)
		{
			const string tagName = "tagName";

			var dict = new Dictionary<string, string>
			{
				{ "prop1", "value1" },
				{ "prop2", "value2" }
			};

			var stringBuilder = new StringBuilder();
			using (var writer = new StringWriter(stringBuilder))
			{
				var underTest = builderFactory(writer);
				underTest.ElementOpenStart(tagName, dict);
			}

			Assert.Equal($"<{tagName} prop1=\"value1\" prop2=\"value2\"", stringBuilder.ToString());
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void TextWriterOutputBuilder_ElementOpen_OpenBracketTagNameCloseBracket(Func<TextWriter, IOutputBuilder> builderFactory)
		{
			const string tagName = "tagName";

			var stringBuilder = new StringBuilder();
			using (var writer = new StringWriter(stringBuilder))
			{
				var underTest = builderFactory(writer);
				underTest.ElementOpen(tagName, null);
			}

			Assert.Equal($"<{tagName}>", stringBuilder.ToString());
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void TextWriterOutputBuilder_ElementOpenWithStaticProperies_OpenBracketTagNameCloseBracket(Func<TextWriter, IOutputBuilder> builderFactory)
		{
			const string tagName = "tagName";

			var dict = new Dictionary<string, string>
			{
				{ "prop1", "value1" },
				{ "prop2", "value2" }
			};

			var stringBuilder = new StringBuilder();
			using (var writer = new StringWriter(stringBuilder))
			{
				var underTest = builderFactory(writer);
				underTest.ElementOpen(tagName, dict);
			}

			Assert.Equal($"<{tagName} prop1=\"value1\" prop2=\"value2\">", stringBuilder.ToString());
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void TextWriterOutputBuilder_ElementOpenEnd_CloseBracket(Func<TextWriter, IOutputBuilder> builderFactory)
		{
			var stringBuilder = new StringBuilder();
			using (var writer = new StringWriter(stringBuilder))
			{
				var underTest = builderFactory(writer);
				underTest.ElementOpenEnd();
			}

			Assert.Equal(">", stringBuilder.ToString());
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void TextWriterOutputBuilder_ElementClose_CloseTag(Func<TextWriter, IOutputBuilder> builderFactory)
		{
			const string tagName = "tagName";

			var stringBuilder = new StringBuilder();
			using (var writer = new StringWriter(stringBuilder))
			{
				var underTest = builderFactory(writer);
				underTest.ElementClose(tagName);
			}

			Assert.Equal($"</{tagName}>", stringBuilder.ToString());
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void TextWriterOutputBuilder_PropertyStart_WhitespaceNameQuote(Func<TextWriter, IOutputBuilder> builderFactory)
		{
			var stringBuilder = new StringBuilder();
			using (var writer = new StringWriter(stringBuilder))
			{
				var underTest = builderFactory(writer);
				underTest.PropertyStart("prop");
			}

			Assert.Equal(" prop=\"", stringBuilder.ToString());
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void TextWriterOutputBuilder_PropertyEnd_Quote(Func<TextWriter, IOutputBuilder> builderFactory)
		{
			var stringBuilder = new StringBuilder();
			using (var writer = new StringWriter(stringBuilder))
			{
				var underTest = builderFactory(writer);
				underTest.PropertyEnd();
			}

			Assert.Equal("\"", stringBuilder.ToString());
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void TextWriterOutputBuilder_Value_PlainText(Func<TextWriter, IOutputBuilder> builderFactory)
		{
			const string value = "value";

			var stringBuilder = new StringBuilder();
			using (var writer = new StringWriter(stringBuilder))
			{
				var underTest = builderFactory(writer);
				underTest.Value(value);
			}

			Assert.Equal(value, stringBuilder.ToString());
		}

		private static IEnumerable<object[]> GetOutputBuilders()
		{
			var builders = new List<Func<TextWriter, IOutputBuilder>>
			{
				BuildRuntimeBuilder,
				BuildCompilationBuilder
			};

			return builders.Select(b => new object[] { b });
		}

		private static IOutputBuilder BuildRuntimeBuilder(TextWriter textWriter)
		{
			return new TextWriterOutputBuilder(textWriter);
		}

		private static IOutputBuilder BuildCompilationBuilder(TextWriter textWriter)
		{
			return new ExpressionToOutputBuilderWrapper(new TextWriterOutputExpressionBuilder(Expression.Constant(textWriter)));
		}
	}
}
