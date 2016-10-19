using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Rendering;
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

			var sequence = new MockSequence();
			var mock = new Mock<IIncrementalDomRenderer>(MockBehavior.Strict);
			foreach (var expr in expressions)
			{
				mock.InSequence(sequence).Setup(expr);
			}

			renderer.Execute(data, new RenderingContext(new IncrementalDomOutput(mock.Object)));

			mock.VerifyAll();
		}
	}
}
