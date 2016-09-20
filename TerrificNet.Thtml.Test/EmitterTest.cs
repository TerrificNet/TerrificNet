using System.Collections.Generic;
using System.Linq.Expressions;
using LightMock;
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

namespace TerrificNet.Thtml.Test
{
	public class EmitterTest
	{
		[Theory]
		[MemberData(nameof(TestData))]
		public void TestEmit(string description, Document input, IDataBinder dataBinder, object data, VTree expected, IHelperBinder helperBinder)
		{
			var method = new ThtmlDocumentCompiler(input, helperBinder).Compile(dataBinder, EmitterFactories.VTree);

			var result = method.Execute(data, null);

			VTreeAsserts.AssertTree(expected, result);
		}

		public static IEnumerable<object[]> TestData
		{
			get
			{
				var nullHelperBinder = new NullHelperBinder();
				yield return new object[]
		  {
					"empty document",
					new Document(),
					new NullDataBinder(),
					null,
					new VNode(),
					nullHelperBinder
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
					nullHelperBinder
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
					nullHelperBinder
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
					nullHelperBinder
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
					nullHelperBinder
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
					nullHelperBinder
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
					nullHelperBinder
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
					nullHelperBinder
				};

				var helperResult = new MockContext<HelperBinderResult>();
				helperResult
					.Arrange(d => d.CreateExpression(The<HelperParameters>.IsAnyValue, The<Expression>.IsAnyValue))
					.Returns<HelperParameters, Expression>((param, expression) => param.OutputExpressionEmitter.HandleTextNode(new TextNode("helper output")));

				var helper = new MockContext<IHelperBinder>();
				helper.Arrange(h => h.FindByName("helper", The<IDictionary<string, string>>.IsAnyValue)).Returns(new HelperBinderResultMock(helperResult));

				yield return new object[]
				{
					"one element with helper",
					new Document(
						new Element("h1", new Statement(new CallHelperExpression("helper")))),
					TypeDataBinder.BinderFromObject(obj3),
					obj3,
					new VNode(
						new VElement("h1", new VText("helper output"))),
					new HelperBinderMock(helper)
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
					nullHelperBinder
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
					nullHelperBinder
				};
			}
		}
	}

	public class Dummy
	{
		public string Name { get; set; }
	}

	public class DummyCollection
	{
		public IEnumerable<Dummy> Items { get; set; }
	}
}