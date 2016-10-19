using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using TerrificNet.Thtml.Formatting;
using TerrificNet.Thtml.Formatting.VDom;
using TerrificNet.Thtml.VDom;
using Xunit;
using System.Linq;
using TerrificNet.Thtml.Test.Extensions;

namespace TerrificNet.Thtml.Test.Formatting
{
	public class VDomOutputBuilderTest
	{
		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void VDomOutputBuilder_ElementOpentStart_InvokesElementOpenStart(Func<IVDomBuilder, IOutputBuilder> builderFactory)
		{
			const string tagName = "tagName";

			var mock = PrepareMock(m => m.ElementOpenStart(tagName));

			var underTest = builderFactory(mock.Object);
			underTest.ElementOpenStart(tagName, null);

			mock.VerifyAll();
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void VDomOutputBuilder_ElementOpentStartWithStaticProperties_InvokesElementOpenStart(Func<IVDomBuilder, IOutputBuilder> builderFactory)
		{
			const string tagName = "tagName";
			var dictionary = new Dictionary<string, string>
			{
				{ "prop1", "val1" },
				{ "prop2", "val2" },
			};

			var mock = PrepareMock(
				m => m.ElementOpenStart(tagName),
				m => m.PropertyStart("prop1"),
				m => m.Value("val1"),
				m => m.PropertyEnd(),
				m => m.PropertyStart("prop2"),
				m => m.Value("val2"),
				m => m.PropertyEnd());

			var underTest = builderFactory(mock.Object);
			underTest.ElementOpenStart(tagName, dictionary);

			mock.VerifyAll();
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void VDomOutputBuilder_ElementOpenEnd_InvokesElementOpenEnd(Func<IVDomBuilder, IOutputBuilder> builderFactory)
		{
			var mock = PrepareMock(
				m => m.ElementOpenEnd());

			var underTest = builderFactory(mock.Object);
			underTest.ElementOpenEnd();

			mock.VerifyAll();
		}

		private static Mock<IVDomBuilder> PrepareMock(params Expression<Action<IVDomBuilder>>[] expressions)
		{
			var mock = new Mock<IVDomBuilder>();
			mock.InSequence(expressions);
			return mock;
		}

		private static IEnumerable<object[]> GetOutputBuilders()
		{
			var builders = new List<Func<IVDomBuilder, IOutputBuilder>>
			{
				BuildRuntimeBuilder,
				BuildCompilationBuilder
			};

			return builders.Select(b => new object[] {b});
		}

		private static IOutputBuilder BuildRuntimeBuilder(IVDomBuilder vDomBuilder)
		{
			return new VDomOutputBuilder(vDomBuilder);
		}

		private static IOutputBuilder BuildCompilationBuilder(IVDomBuilder vDomBuilder)
		{
			return new ExpressionToOutputBuilderWrapper(new VDomOutputExpressionBuilder(Expression.Constant(vDomBuilder)));
		}
	}
}
