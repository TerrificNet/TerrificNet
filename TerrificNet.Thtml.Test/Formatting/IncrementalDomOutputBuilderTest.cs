using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using TerrificNet.Thtml.Formatting;
using TerrificNet.Thtml.Formatting.IncrementalDom;
using TerrificNet.Thtml.Rendering;
using TerrificNet.Thtml.Test.Extensions;
using Xunit;

namespace TerrificNet.Thtml.Test.Formatting
{
	public class IncrementalDomOutputBuilderTest
	{
		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void IncrementalDomOutputBuilder_ElementOpenStart_InvokesElementOpenStart(Func<IIncrementalDomRenderer, IOutputBuilder> builderFactory)
		{
			const string tagName = "tagName";
			var dict = new Dictionary<string, string>();

			var mock = PrepareMock(m => m.ElementOpenStart(tagName, null, dict, null));

			var underTest = builderFactory(mock.Object);
			underTest.ElementOpenStart(tagName, dict);

			mock.VerifyAll();
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void IncrementalDomOutputBuilder_ElementOpen_InvokesElementOpen(Func<IIncrementalDomRenderer, IOutputBuilder> builderFactory)
		{
			const string tagName = "tagName";
			var dict = new Dictionary<string, string>();

			var mock = PrepareMock(m => m.ElementOpen(tagName, null, dict, null));

			var underTest = builderFactory(mock.Object);
			underTest.ElementOpen(tagName, dict);

			mock.VerifyAll();
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void IncrementalDomOutputBuilder_ElementOpenEnd_InvokesElementOpenEnd(Func<IIncrementalDomRenderer, IOutputBuilder> builderFactory)
		{
			var mock = PrepareMock(m => m.ElementOpenEnd());

			var underTest = builderFactory(mock.Object);
			underTest.ElementOpenEnd();

			mock.VerifyAll();
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void IncrementalDomOutputBuilder_ElementEnd_InvokesElementEnd(Func<IIncrementalDomRenderer, IOutputBuilder> builderFactory)
		{
			const string tagName = "tagName";
			var mock = PrepareMock(m => m.ElementClose(tagName));

			var underTest = builderFactory(mock.Object);
			underTest.ElementClose(tagName);

			mock.VerifyAll();
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void IncrementalDomOutputBuilder_Value_InvokesText(Func<IIncrementalDomRenderer, IOutputBuilder> builderFactory)
		{
			const string value = "tagName";
			var mock = PrepareMock(m => m.Text(value));

			var underTest = builderFactory(mock.Object);
			underTest.Value(value);

			mock.VerifyAll();
		}

		[Theory]
		[MemberData(nameof(GetOutputBuilders))]
		public void IncrementalDomOutputBuilder_PropertyStartValuePropertyEnd_InvokesAttr(Func<IIncrementalDomRenderer, IOutputBuilder> builderFactory)
		{
			const string propertyName = "propertyName";
			const string value1 = "val";
			const string value2 = "val";
			var mock = PrepareMock(m => m.Attr(propertyName, value1 + value2));

			var underTest = builderFactory(mock.Object);

			underTest.PropertyStart(propertyName);
			underTest.Value(value1);
			underTest.Value(value2);
			underTest.PropertyEnd();

			mock.VerifyAll();
		}

		private static Mock<IIncrementalDomRenderer> PrepareMock(params Expression<Action<IIncrementalDomRenderer>>[] expressions)
		{
			var mock = new Mock<IIncrementalDomRenderer>();
			mock.InSequence(expressions);
			return mock;
		}

		private static IEnumerable<object[]> GetOutputBuilders()
		{
			var builders = new List<Func<IIncrementalDomRenderer, IOutputBuilder>>
			{
				BuildRuntimeBuilder,
				BuildCompilationBuilder
			};

			return builders.Select(b => new object[] { b });
		}

		private static IOutputBuilder BuildRuntimeBuilder(IIncrementalDomRenderer renderer)
		{
			return new IncrementalDomOutputBuilder(renderer);
		}

		private static IOutputBuilder BuildCompilationBuilder(IIncrementalDomRenderer renderer)
		{
			return new ExpressionToOutputBuilderWrapper(new IncrementalDomOutputExpressionBuilder(Expression.Constant(renderer)));
		}
	}
}
