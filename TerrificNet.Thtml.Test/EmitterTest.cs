using System.Collections.Generic;
using System.IO;
using System.Text;
using LightMock;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.Test.Asserts;
using TerrificNet.Thtml.VDom;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class EmitterTest
	{
		[Theory]
		[MemberData("TestData")]
		public void TestEmit(string description, Document input, IDataScopeContract dataScopeContract, object data, VTree expected, IHelperBinder helperBinder)
		{
			var compiler = new VTreeEmitter();
			var method = compiler.Emit(input, dataScopeContract, helperBinder);

			var result = method.Execute(new ObjectDataContext(data), null);

			VTreeAsserts.AssertTree(expected, result);
		}

		[Theory]
		[MemberData("TestData")]
		public void TestStreamEmit(string description, Document input, IDataScopeContract dataScopeContract, object data, VTree expected, IHelperBinder helperBinder)
		{
			var compiler = new VTreeEmitter();
			var method = compiler.Emit(input, dataScopeContract, helperBinder);
			var result = method.Execute(new ObjectDataContext(data), null);
			var expectedString = result.ToString();


			// hack 
			var helperResult = new MockContext<HelperBinderResult>();
			helperResult.Arrange(d => d.CreateEmitter(The<IListEmitter<StreamWriterHandler>>.IsAnyValue, The<IHelperBinder>.IsAnyValue, The<IDataScopeContract>.IsAnyValue))
				.Returns(EmitterNode.AsList(EmitterNode.Lambda<StreamWriterHandler>((d, r) => (writer => writer.Write("helper output")))));

			var helper = new MockContext<IHelperBinder>();
			helper.Arrange(h => h.FindByName("helper", The<IDictionary<string, string>>.IsAnyValue)).Returns(new HelperBinderResultMock(helperResult));

			var streamCompiler = new StreamEmitter();
			var sb = new StringBuilder();
			streamCompiler.Emit(input, dataScopeContract, new HelperBinderMock(helper)).Execute(new ObjectDataContext(data), null)(new StringWriter(sb));

			Assert.Equal(expectedString, sb.ToString());
		}

		public static IEnumerable<object[]> TestData
		{
			get
			{
				yield return new object[]
				{
					"empty document",
					new Document(),
					new DataScopeContractLegacyWrapper(new NullDataScope()),
					null,
					new VNode(),
					new NullHelperBinder()
				};

				yield return new object[]
				{
					"one element",
					new Document(
						new Element("h1",
							new TextNode("hallo"))),
					new DataScopeContractLegacyWrapper(new NullDataScope()),
					null,
					new VNode(
						new VElement("h1",
							new VText("hallo"))),
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
					new DataScopeContractLegacyWrapper(new NullDataScope()),
					null,
					new VNode(
						new VElement("h1", new[] { new VProperty("attr1", new StringVPropertyValue("hallo")) },
							new VElement("h2", new[] { new VProperty("attr2", new StringVPropertyValue("hallo2")) }),
							new VElement("h3", new[] { new VProperty("attr3", new StringVPropertyValue("hallo3")) })),
						new VElement("h1", new[] { new VProperty("attr4", new StringVPropertyValue("hallo4")) })),
					new NullHelperBinder()
				};

				var obj = new Dummy { Name = "hallo" };
				yield return new object[]
				{
					"one element with dynamic expression",
					new Document(
						new Element("h1",
							new Statement(new MemberExpression("name")))),
					new DataScopeContractLegacyWrapper(TypeDataScope.BinderFromObject(obj)),
					obj,
					new VNode(
						new VElement("h1",
							new VText("hallo"))),
					new NullHelperBinder()
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
					new DataScopeContractLegacyWrapper(TypeDataScope.BinderFromObject(obj2)),
					obj2,
					new VNode(
						new VElement("h1",
							new VElement("div",
								new VText("test1")),
							new VElement("div",
								new VText("test2")))),
					new NullHelperBinder()
				};

				var obj3 = new Dummy { Name = "value" };
				yield return new object[]
				{
					"one element with attribute expression",
					new Document(
						new Element("h1", new ElementPart[] { new AttributeNode("title", new AttributeContentStatement(new MemberExpression("name"))) })),
					new DataScopeContractLegacyWrapper(TypeDataScope.BinderFromObject(obj3)),
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
					new DataScopeContractLegacyWrapper(TypeDataScope.BinderFromObject(obj4)),
					obj4,
					new VNode(
						new VElement("h1", new VText("hallo2"))
						),
					new NullHelperBinder()
				};

				var result = new MockContext<HelperBinderResult>();
				result.Arrange(d => d.CreateEmitter(The<IListEmitter<VTree>>.IsAnyValue, The<IHelperBinder>.IsAnyValue, The<IDataScopeContract>.IsAnyValue))
					.Returns(EmitterNode.AsList(EmitterNode.Lambda((d, r) => new VText("helper output"))));

				var helper = new MockContext<IHelperBinder>();
				helper.Arrange(h => h.FindByName("helper", The<IDictionary<string, string>>.IsAnyValue)).Returns(new HelperBinderResultMock(result));

				yield return new object[]
				{
					"one element with helper",
					new Document(
						new Element("h1", new Statement(new CallHelperExpression("helper")))),
					new DataScopeContractLegacyWrapper(TypeDataScope.BinderFromObject(obj3)),
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
					new DataScopeContractLegacyWrapper(TypeDataScope.BinderFromObject(obj5)),
					obj5,
					new VNode(
						new VElement("h1", new [] { new VProperty("test", new StringVPropertyValue("memberhallo")) })),
					new NullHelperBinder()
				};

				var obj6 = new { Do = true };
				yield return new object[]
				{
					"one element with conditional attribte",
					new Document(
						new Element("h1", new ElementPart[]
						{
							new AttributeNode("test",
								new AttributeContentStatement(
									new ConditionalExpression(new MemberExpression("do")),
									new ConstantAttributeContent("hallo")))
						})),
					new DataScopeContractLegacyWrapper(TypeDataScope.BinderFromObject(obj6)),
					obj6,
					new VNode(
						new VElement("h1", new [] { new VProperty("test", new StringVPropertyValue("hallo")) })),
					new NullHelperBinder()
				};
			}
		}
	}

	internal class HelperBinderMock : IHelperBinder
	{
		private readonly IInvocationContext<IHelperBinder> _invocationContext;

		public HelperBinderMock(IInvocationContext<IHelperBinder> invocationContext)
		{
			_invocationContext = invocationContext;
		}

		public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
		{
			return _invocationContext.Invoke(f => f.FindByName(helper, arguments));
		}
	}

	internal class HelperBinderResultMock : HelperBinderResult
	{
		private readonly IInvocationContext<HelperBinderResult> _invocationContext;

		public HelperBinderResultMock(IInvocationContext<HelperBinderResult> invocationContext)
		{
			_invocationContext = invocationContext;
		}

		public override IListEmitter<T> CreateEmitter<T>(IListEmitter<T> children, IHelperBinder helperBinder, IDataScopeContract scopeContract)
		{
			return _invocationContext.Invoke(f => f.CreateEmitter(children, helperBinder, scopeContract));
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