using TerrificNet.Thtml.Test.Asserts;
using TerrificNet.Thtml.VDom;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class VDomBuilderTest
	{
		[Fact]
		public void VDomBuilder_Nothing_ReturnsEmptyNode()
		{
			var expected = new VNode();

			var underTest = new VDomBuilder();

			var result = underTest.ToDom();
			VTreeAsserts.AssertTree(expected, result);
		}

		[Fact]
		public void VDomBuilder_SingleElementCall_ReturnsNodeWithOneElement()
		{
			var expected = new VNode(new VElement("t1"));

			var underTest = new VDomBuilder();
			underTest.ElementOpen("t1");
			underTest.ElementClose();

			var result = underTest.ToDom();
			VTreeAsserts.AssertTree(expected, result);
		}

		[Fact]
		public void VDomBuilder_NestedElementCall_ReturnsNodeWithOneElementAndChild()
		{
			var expected = new VNode(new VElement("t1", new VElement("t2")));

			var underTest = new VDomBuilder();
			underTest.ElementOpen("t1");
			underTest.ElementOpen("t2");
			underTest.ElementClose();
			underTest.ElementClose();

			var result = underTest.ToDom();
			VTreeAsserts.AssertTree(expected, result);
		}

		[Fact]
		public void VDomBuilder_ElementOpenStartAddPropertyElementOpenEndElementClose_ReturnsNodeWithOneElementAndProperties()
		{
			var expected = new VNode(new VElement("t1", new[] { new VProperty("attr1", new StringVPropertyValue("hallo")) }));

			var underTest = new VDomBuilder();
			underTest.ElementOpenStart("t1");
			underTest.PropertyStart("attr1");
			underTest.Value("hallo");
			underTest.PropertyEnd();
			underTest.ElementOpenEnd();
			underTest.ElementClose();

			var result = underTest.ToDom();
			VTreeAsserts.AssertTree(expected, result);
		}

		[Fact]
		public void VDomBuilder_Value_ReturnsSingleTextNode()
		{
			var expected = new VNode(new VText("hallo"));

			var underTest = new VDomBuilder();
			underTest.Value("hallo");

			var result = underTest.ToDom();
			VTreeAsserts.AssertTree(expected, result);
		}

		[Fact]
		public void VDomBuilder_ElementOpenValueElementClose_ReturnsSingleElementWithSingleTextNode()
		{
			var expected = new VNode(new VElement("h1", new VText("hallo")));

			var underTest = new VDomBuilder();
			underTest.ElementOpen("h1");
			underTest.Value("hallo");
			underTest.ElementClose();

			var result = underTest.ToDom();
			VTreeAsserts.AssertTree(expected, result);
		}

		[Fact]
		public void VDomBuilder_TwoFlatElementsWithAttribute_ReturnsIndependentElements()
		{
			var expected = new VNode(
				new VElement("h1", new[] { new VProperty("attr1", new StringVPropertyValue("hallo")) }),
				new VElement("h2", new[] { new VProperty("attr2", new StringVPropertyValue("hallo2")) }));

			var underTest = new VDomBuilder();
			underTest.ElementOpenStart("h1");
			underTest.PropertyStart("attr1");
			underTest.Value("hallo");
			underTest.PropertyEnd();
			underTest.ElementOpenEnd();
			underTest.ElementClose();

			underTest.ElementOpenStart("h2");
			underTest.PropertyStart("attr2");
			underTest.Value("hallo2");
			underTest.PropertyEnd();
			underTest.ElementOpenEnd();
			underTest.ElementClose();

			var result = underTest.ToDom();
			VTreeAsserts.AssertTree(expected, result);
		}

		[Fact]
		public void VDomBuilder_NestedElementsWithAttribute_ReturnsIndependentElements()
		{
			var expected = new VNode(
				new VElement("h1", new[] { new VProperty("attr1", new StringVPropertyValue("hallo")) },
					new VElement("h2", new[] { new VProperty("attr2", new StringVPropertyValue("hallo2")) })));

			var underTest = new VDomBuilder();
			underTest.ElementOpenStart("h1");
			underTest.PropertyStart("attr1");
			underTest.Value("hallo");
			underTest.PropertyEnd();
			underTest.ElementOpenEnd();

			underTest.ElementOpenStart("h2");
			underTest.PropertyStart("attr2");
			underTest.Value("hallo2");
			underTest.PropertyEnd();
			underTest.ElementOpenEnd();
			underTest.ElementClose();

			underTest.ElementClose();

			var result = underTest.ToDom();
			VTreeAsserts.AssertTree(expected, result);
		}
	}
}
