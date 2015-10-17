using System.Collections.Generic;
using System.IO;
using System.Text;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class IlEmitterTest
	{
		[Theory]
		[MemberData("TestData")]
		public void TestEmit(string description, Document input, IDataBinder dataBinder, object data, string expected, IHelperBinder helperBinder)
		{
			var compiler = new IlEmitter();
			var method = compiler.Emit(input, dataBinder, helperBinder);

			var sb = new StringBuilder();
			var writer = new StringWriter(sb);
			method.Execute(new ObjectDataContext(data), null)(writer);

			Assert.Equal(expected, sb.ToString());
		}

		public static IEnumerable<object[]> TestData
		{
			get
			{
				yield return new object[]
				{
					"empty document",
					new Document(),
					new NullDataBinder(),
					null,
					"",
					new NullHelperBinder()
				};

				yield return new object[]
				{
					"one element",
					new Document(
						new Element("h1",
							new TextNode("hallo"))),
					new NullDataBinder(),
					null,
					"<h1>hallo</h1>",
					new NullHelperBinder()
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
					"<h1 attr1=\"hallo\"><h2 attr2=\"hallo2\"></h2><h3 attr3=\"hallo3\"></h3></h1><h1 attr4=\"hallo4\"></h1>",
					new NullHelperBinder()
				};

				var obj = new { Name = "hallo" };
				yield return new object[]
				{
					"one element with dynamic expression",
					new Document(
						new Element("h1",
							new Statement(new MemberExpression("name")))),
					TypeDataBinder.BinderFromObject(obj),
					obj,
					"<h1>hallo</h1>",
					new NullHelperBinder()
				};

				/*var obj2 = new
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
					new NullHelperBinder()
				};
				var obj3 = new { Name = "value" };
				yield return new object[]
				{
					"one element with attribute expression",
					new Document(
						new Element("h1", new ElementPart[] { new AttributeNode("title", new AttributeContentStatement(new MemberExpression("name"))) })),
					TypeDataBinder.BinderFromObject(obj3),
					obj3,
					new VNode(
						new VElement("h1", new[] { new VProperty("title", new StringVPropertyValue("value")) }, null)),
					new NullHelperBinder()
				};

				var obj4 = new
				{
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
								new Element("h1", new Statement(new MemberExpression("value"))))
							)),
					TypeDataBinder.BinderFromObject(obj4),
					obj4,
					new VNode(
						new VElement("h1", new VText("hallo2"))
						),
					new NullHelperBinder()
				};

				var result = new Mock<HelperBinderResult>(MockBehavior.Loose);
				result.Setup(d => d.CreateEmitter(It.IsAny<IListEmitter<VTree>>(), It.IsAny<IHelperBinder>(), It.IsAny<IDataBinder>())).Returns(EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText("helper output"))));

				var helper = new Mock<IHelperBinder>();
				helper.Setup(h => h.FindByName("helper", It.IsAny<IDictionary<string, string>>())).Returns(result.Object);

				yield return new object[]
				{
					"one element with helper",
					new Document(
						new Element("h1", new Statement(new CallHelperExpression("helper")))),
					TypeDataBinder.BinderFromObject(obj3),
					obj3,
					new VNode(
						new VElement("h1", new VText("helper output"))),
					helper.Object
				};*/
			}
		}
	}
}