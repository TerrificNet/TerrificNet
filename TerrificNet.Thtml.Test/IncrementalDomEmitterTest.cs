using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Formatting;
using TerrificNet.Thtml.Formatting.IncrementalDom;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Rendering;
using TerrificNet.Thtml.Test.Extensions;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class IncrementalDomEmitterTest
	{
		[Fact]
		public void IncrementalDom_SingleElement_CallsElementVoid()
		{
			var document = new Document(new Element("div"));
			var expectedCalls = new List<Expression<Action<IIncrementalDomRenderer>>>
			{
				s => s.ElementOpen("div", null, It.IsAny<Dictionary<string, string>>(), null),
				s => s.ElementClose("div")
			};
			object data = null;

			Test(document, expectedCalls, data);
		}

		[Fact]
		public void IncrementalDom_SingleElementWithNestedChild_CallsOpenAndCloseElement()
		{
			var document = new Document(new Element("div", new Element("a")));
			var expectedCalls = new List<Expression<Action<IIncrementalDomRenderer>>>
			{
				s => s.ElementOpen("div", null, It.IsAny<Dictionary<string, string>>(), null),
				s => s.ElementOpen("a", null, It.IsAny<Dictionary<string, string>>(), null),
				s => s.ElementClose("a"),
				s => s.ElementClose("div")
			};
			object data = null;

			Test(document, expectedCalls, data);
		}

		[Fact]
		public void IncrementalDom_SingleElementWithStaticAttributes_CallsOpenElementStartAndCloseElement()
		{
			var document = new Document(new Element("div", new [] { new AttributeNode("test", new ConstantAttributeContent("blau")) }));
			var expectedCalls = new List<Expression<Action<IIncrementalDomRenderer>>>
			{
				s => s.ElementOpen("div", null, new Dictionary<string, string> { { "test", "blau" }}, null),
				s => s.ElementClose("div")
			};
			object data = null;

			Test(document, expectedCalls, data);
		}

		[Fact]
		public void IncrementalDom_SingleElementWithDynamicAttributes_CallsOpenElementStartAndCloseElement()
		{
			object data = new { value = "blau" };

			var document = new Document(new Element("div", new[] { new AttributeNode("test", new AttributeContentStatement(new Parsing.Handlebars.MemberExpression("value"))) }));
			var expectedCalls = new List<Expression<Action<IIncrementalDomRenderer>>>
			{
				s => s.ElementOpenStart("div", null, It.IsAny<Dictionary<string, string>>(), null),
				s => s.Attr("test", "blau"),
				s => s.ElementOpenEnd(),
				s => s.ElementClose("div")
			};

			Test(document, expectedCalls, data);
		}

		[Fact]
		public void IncrementalDom_SingleTextNode_CallsTextNode()
		{
			var document = new Document(new TextNode("hallo"));
			var expectedCalls = new List<Expression<Action<IIncrementalDomRenderer>>>
			{
				s => s.Text("hallo")
			};
			object data = null;

			Test(document, expectedCalls, data);
		}

		[Fact]
		public void IncrementalDom_SingleCallExpression_CallsTextNode()
		{
			var data = new { value = "hallo"};
			var document = new Document(new Statement(new Parsing.Handlebars.MemberExpression("value")));
			var expectedCalls = new List<Expression<Action<IIncrementalDomRenderer>>>
			{
				s => s.Text("hallo")
			};

			Test(document, expectedCalls, data);
		}

		private static void Test(Document document, IEnumerable<Expression<Action<IIncrementalDomRenderer>>> expressions, object data)
		{
			var compiler = new ThtmlDocumentCompiler(document, CompilerExtensions.Default);
			var renderer = compiler.Compile(new DynamicDataBinder(), OutputFactories.IncrementalDomScript);

			var mock = new Mock<IIncrementalDomRenderer>(MockBehavior.Strict);
			mock.InSequence(expressions);

			renderer.Execute(data, new RenderingContext(new IncrementalDomOutputBuilder(mock.Object)));

			mock.VerifyAll();
		}
	}
}
