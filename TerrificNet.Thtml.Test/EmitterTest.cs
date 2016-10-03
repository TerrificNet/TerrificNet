using System.Collections.Generic;
using Moq;
using TerrificNet.Thtml.Binding;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.Test.Asserts;
using TerrificNet.Thtml.Test.Stubs;
using TerrificNet.Thtml.VDom;
using Xunit;
using ConditionalExpression = TerrificNet.Thtml.Parsing.Handlebars.ConditionalExpression;
using MemberExpression = TerrificNet.Thtml.Parsing.Handlebars.MemberExpression;
using System.Linq;

namespace TerrificNet.Thtml.Test
{
	public class EmitterTest
	{
		[Theory]
		[MemberData(nameof(TestData))]
		public void TestEmit(string description, Document input, IDataBinder dataBinder, object data, VTree expected, CompilerExtensions compilerExtensions)
		{
			var method = new ThtmlDocumentCompiler(input, compilerExtensions).Compile(dataBinder, EmitterFactories.VTree);

			var result = method.Execute(data, null);

			VTreeAsserts.AssertTree(expected, result);
		}

		public static IEnumerable<object[]> TestData
		{
			get
			{
				var compilerExtensions = CompilerExtensions.Default;
				yield return new object[]
				{
					"empty document",
					new Document(),
					new NullDataBinder(),
					null,
					new VNode(),
					compilerExtensions
				};

				yield return new object[]
				{
					"one element",
					new Document(
						new Element("h1",
							new TextNode("hallo"))),
					new NullDataBinder(),
					null,
					new VNode(
						new VElement("h1",
							new VText("hallo"))),
					compilerExtensions
				};

				yield return new object[]
				{
					"element with attributes",
					new Document(
						new Element("h1", new ElementPart [] { new AttributeNode("attr1", new ConstantAttributeContent("hallo")) },
							new Element("h2", new ElementPart [] { new AttributeNode("attr2", new ConstantAttributeContent("hallo2")) }),
							new Element("h3", new ElementPart [] { new AttributeNode("attr3", new ConstantAttributeContent("hallo3")) })
						),
						new Element("h1", new ElementPart [] { new AttributeNode("attr4", new ConstantAttributeContent("hallo4")) })),
					new NullDataBinder(),
					null,
					new VNode(
						new VElement("h1", new[] { new VProperty("attr1", new StringVPropertyValue("hallo")) },
							new VElement("h2", new[] { new VProperty("attr2", new StringVPropertyValue("hallo2")) }),
							new VElement("h3", new[] { new VProperty("attr3", new StringVPropertyValue("hallo3")) })),
						new VElement("h1", new[] { new VProperty("attr4", new StringVPropertyValue("hallo4")) })),
					compilerExtensions
				};

				var obj = new Dummy { Name = "hallo" };
				yield return new object[]
				{
					"one element with dynamic expression",
					new Document(
						new Element("h1",
							new Statement(new MemberExpression("name")))),
					TypeDataBinder.BinderFromObject(obj),
					obj,
					new VNode(
						new VElement("h1",
							new VText("hallo"))),
					compilerExtensions
				};

				var obj2 = new DummyCollection
				{
					Items = new[] { new Dummy { Name = "test1" }, new Dummy { Name = "test2" } }
				};
				yield return new object[]
				{
					"one element with iteration expression",
					new Document(
						new Element("h1",
							new Statement(
								new IterationExpression(new MemberExpression("items")),
								new Element("div",
									new Statement(new MemberExpression("name")))))),
					TypeDataBinder.BinderFromObject(obj2),
					obj2,
					new VNode(
						new VElement("h1",
							new VElement("div",
								new VText("test1")),
							new VElement("div",
								new VText("test2")))),
					compilerExtensions
				};

				var obj3 = new Dummy { Name = "value" };
				yield return new object[]
				{
					"one element with attribute expression",
					new Document(
						new Element("h1", new ElementPart[] { new AttributeNode("title", new AttributeContentStatement(new MemberExpression("name"))) })),
					TypeDataBinder.BinderFromObject(obj3),
					obj3,
					new VNode(
						new VElement("h1", new[] { new VProperty("title", new StringVPropertyValue("value")) }, null)),
					compilerExtensions
				};

				var obj4 = new
				{
					Parent = "test",
					List = new[]
					{
					  new { Do = false, Value = "hallo1" },
					  new { Do = true, Value = "hallo2" },
					}
				};

				yield return new object[]
				{
					"one element with iteration and including conditional expression",
					new Document(
						new Statement(new IterationExpression(new MemberExpression("list")),
							new Statement(new ConditionalExpression(new MemberExpression("do")),
								new Element("h1", new Statement(new ParentExpression(new MemberExpression("parent"))), new Statement(new MemberExpression("value"))))
						)),
					TypeDataBinder.BinderFromObject(obj4),
					obj4,
					new VNode(
						new VElement("h1", new VText("test"), new VText("hallo2"))
						),
					compilerExtensions
				};

				var obj7 = new
				{
					Value = "1",
					Sub = new
					{
						Value = "2",
						Sub = new
						{
							Value = "3"
						}
					}
				};

				yield return new object[]
				{
					"text with nested expressions",
					new Document(
						new Statement(new MemberExpression("value")),
						new Statement(new MemberExpression("sub", new MemberExpression("value"))),
						new Statement(new MemberExpression("sub", new MemberExpression("sub", new MemberExpression("value"))))),
					TypeDataBinder.BinderFromObject(obj7),
					obj7,
					new VNode(new VText("1"), new VText("2"), new VText("3")),
					compilerExtensions
				};

				var helperResult = new Mock<HelperBinderResult>();
				helperResult
					.Setup(d => d.CreateExpression(It.IsAny<HelperParameters>()))
					.Returns<HelperParameters>(param => param.CompilerExtensions.OutputEmitter.HandleTextNode(new TextNode("helper output")));

				var helper = new Mock<IHelperBinder>();
				helper.Setup(h => h.FindByName("helper", It.IsAny<IDictionary<string, string>>())).Returns(helperResult.Object);

				yield return new object[]
				{
					"one element with helper",
					new Document(
						new Element("h1", new Statement(new CallHelperExpression("helper")))),
					TypeDataBinder.BinderFromObject(obj3),
					obj3,
					new VNode(
						new VElement("h1", new VText("helper output"))),
					CompilerExtensions.Default.AddHelperBinder(helper.Object)
				};

				var obj5 = new { Member = "member" };
				yield return new object[]
				{
					"one element with composite attribute",
					new Document(
						new Element("h1", new ElementPart[]
						{
							new AttributeNode("test",
								new CompositeAttributeContent(
									new AttributeContentStatement(new MemberExpression("member")),
									new ConstantAttributeContent("hallo")))
						})),
					TypeDataBinder.BinderFromObject(obj5),
					obj5,
					new VNode(
						new VElement("h1", new [] { new VProperty("test", new StringVPropertyValue("memberhallo")) })),
					compilerExtensions
				};

				var obj6 = new { Do = true };
				yield return new object[]
				{
					"one element with conditional attribute",
					new Document(
						new Element("h1", new ElementPart[]
						{
							new AttributeNode("test",
								new AttributeContentStatement(
									new ConditionalExpression(new MemberExpression("do")),
									new ConstantAttributeContent("hallo")))
						})),
					TypeDataBinder.BinderFromObject(obj6),
					obj6,
					new VNode(
						new VElement("h1", new [] { new VProperty("test", new StringVPropertyValue("hallo")) })),
					compilerExtensions
				};

				var resultMock = new Mock<HelperBinderResult>();
				resultMock.Setup(r => r.CreateExpression(It.IsAny<HelperParameters>())).Returns((HelperParameters p) =>
				{
					var ex1 = p.CompilerExtensions.OutputEmitter.HandleTextNode(new TextNode("test"));
					var ex2 = p.CompilerExtensions.OutputEmitter.HandleTextNode(new TextNode("test2"));

					return p.CompilerExtensions.OutputEmitter.HandleElementList(new[] { ex1, ex2 }.ToList());
				});

				var helperMock = new Mock<IHelperBinder>();
				helperMock.Setup(h => h.FindByName("helper", It.IsAny<IDictionary<string, string>>())).Returns(resultMock.Object);

				yield return new object[]
				{
					"one element with binder returning multiple children",
					new Document(
						new Element("h1", new Statement(new CallHelperExpression("helper")))),
					new NullDataBinder(),
					null,
					new VNode(
						new VElement("h1", new VText("test"), new VText("test2"))
						),
					CompilerExtensions.Default.AddHelperBinder(helperMock.Object)
				};

				var elementNode = new Element("mixin:test", new TextNode("test1"));

				var tagHelper = new Mock<ITagHelper>();
				tagHelper.Setup(t => t.FindByName(elementNode)).Returns(HelperBinderResult.Create(param => elementNode.ChildNodes[0].Accept(param.Visitor)));

				yield return new object[]
				{
					"one element with tag helper and children",
					new Document(elementNode),
					new NullDataBinder(),
					null,
					new VNode(
						new VText("test1")
						),
					CompilerExtensions.Default.AddTagHelper(tagHelper.Object)
				};
			}
		}
	}
}