using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using LightMock;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Emit.Compiler;
using TerrificNet.Thtml.Emit.Schema;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using Xunit;
using ConditionalExpression = TerrificNet.Thtml.Parsing.Handlebars.ConditionalExpression;
using MemberExpression = TerrificNet.Thtml.Parsing.Handlebars.MemberExpression;

namespace TerrificNet.Thtml.Test
{
	public class StreamEmitterTest
	{
		[Theory]
		[MemberData("TestData")]
		public void TestExpressionEmit(string description, Document input, IDataScopeContract dataScopeContract, object data, string expected, IHelperBinder<Expression, ExpressionHelperConfig> helperBinder)
		{
			var streamCompiler = new ExpressionEmitter();
			var sb = new StringBuilder();
			streamCompiler.Emit(input, dataScopeContract, helperBinder).Execute(data, null)(new StringWriter(sb));

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
					new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), new NullDataScope()),
					null,
					"",
					new NullHelperBinder<Expression, ExpressionHelperConfig>()
				};

				yield return new object[]
				{
					"one element",
					new Document(
						new Element("h1",
							new TextNode("hallo"))),
					new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), new NullDataScope()),
					null,
					"<h1>hallo</h1>",
					new NullHelperBinder<Expression, ExpressionHelperConfig>()
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
					new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), new NullDataScope()),
					null,
					"<h1 attr1=\"hallo\"><h2 attr2=\"hallo2\"></h2><h3 attr3=\"hallo3\"></h3></h1><h1 attr4=\"hallo4\"></h1>",
					new NullHelperBinder<Expression, ExpressionHelperConfig>()
				};

				var obj = new Dummy { Name = "hallo" };
				yield return new object[]
				{
					"one element with dynamic expression",
					new Document(
						new Element("h1",
							new Statement(new MemberExpression("name")))),
					new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), TypeDataScope.BinderFromObject(obj)),
					obj,
					"<h1>hallo</h1>",
					new NullHelperBinder<Expression, ExpressionHelperConfig>()
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
					new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), TypeDataScope.BinderFromObject(obj2)),
					obj2,
					"<h1><div>test1</div><div>test2</div></h1>",
					new NullHelperBinder<Expression, ExpressionHelperConfig>()
				};

				var obj3 = new Dummy { Name = "value" };
				yield return new object[]
				{
					"one element with attribute expression",
					new Document(
						new Element("h1", new ElementPart[] { new AttributeNode("title", new AttributeContentStatement(new MemberExpression("name"))) })),
					new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), TypeDataScope.BinderFromObject(obj3)),
					obj3,
					"<h1 title=\"value\"></h1>",
					new NullHelperBinder<Expression, ExpressionHelperConfig>()
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
					new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), TypeDataScope.BinderFromObject(obj4)),
					obj4,
					"<h1>hallo2</h1>",
					new NullHelperBinder<Expression,ExpressionHelperConfig>()
				};

				var helperResult = new MockContext<HelperBinderResult<Expression, ExpressionHelperConfig>>();
				helperResult.Arrange(d => d.CreateEmitter(The<StreamOutputExpressionEmitter>.IsAnyValue, The<Expression>.IsAnyValue, The<IHelperBinder<Expression, ExpressionHelperConfig>>.IsAnyValue, The<IDataScopeContract>.IsAnyValue))
					.Returns<StreamOutputExpressionEmitter, Expression, IHelperBinder<Expression, ExpressionHelperConfig>, IDataScopeContract>((handler, expression, arg3, arg4) => handler.HandleTextNode(new TextNode("helper output")));

				var helper = new MockContext<IHelperBinder<Expression, ExpressionHelperConfig>>();
				helper.Arrange(h => h.FindByName("helper", The<IDictionary<string, string>>.IsAnyValue)).Returns(new HelperBinderResultMock<Expression, ExpressionHelperConfig>(helperResult));

				yield return new object[]
				{
					"one element with helper",
					new Document(
						new Element("h1", new Statement(new CallHelperExpression("helper")))),
					new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), TypeDataScope.BinderFromObject(obj3)),
					obj3,
					"<h1>helper output</h1>",
					new HelperBinderMock<Expression,ExpressionHelperConfig>(helper)
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
					new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), TypeDataScope.BinderFromObject(obj5)),
					obj5,
					"<h1 test=\"memberhallo\"></h1>",
					new NullHelperBinder<Expression,ExpressionHelperConfig>()
				};

				var obj6 = new { Do = true, Dont = false };
				yield return new object[]
				{
					"one element with conditional attribute",
					new Document(
						new Element("h1", new ElementPart[]
						{
							new AttributeNode("test",
								new AttributeContentStatement(
									new ConditionalExpression(new MemberExpression("do")),
									new ConstantAttributeContent("hallo"))),
							new AttributeNode("test2",
								new AttributeContentStatement(
									new ConditionalExpression(new MemberExpression("dont")),
									new ConstantAttributeContent("hallo")))
						})),
					new DataScopeContractLegacyWrapper(new DataScopeContract("_global"), TypeDataScope.BinderFromObject(obj6)),
					obj6,
					"<h1 test=\"hallo\" test2=\"\"></h1>",
					new NullHelperBinder<Expression,ExpressionHelperConfig>()
				};
			}
		}
	}
}