using TerrificNet.Thtml.VDom;
using Xunit;

namespace TerrificNet.Thtml.Test.Asserts
{
    public static class VTreeAsserts
    {
        public static void AssertTree(VTree expected, VTree result)
        {
            if (expected == null)
            {
                Assert.Null(result);
                return;
            }

	        GenericAssert.AssertOneOf(
                () => GenericAssert.AssertValue<VText>(expected, result, AssertText),
                () => GenericAssert.AssertValue<VElement>(expected, result, AssertElement),
                () => GenericAssert.AssertValue<VNode>(expected, result, AssertNode));
        }

	    private static void AssertElement(VElement expected, VElement result)
        {
            Assert.Equal(expected.TagName, result.TagName);
		    GenericAssert.AssertCollection(expected.Children, result.Children, AssertTree);
		    GenericAssert.AssertCollection(expected.PropertyList, result.PropertyList, AssertProperty);
        }

        private static void AssertProperty(VProperty expected, VProperty actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

	        GenericAssert.AssertOneOf(
                () => GenericAssert.AssertValue<StringVPropertyValue>(expected.Value, actual.Value, AssertStringValue),
                () => GenericAssert.AssertValue<BooleanVPropertyValue>(expected.Value, actual.Value, AssertBooleanValue),
                () => GenericAssert.AssertValue<NumberVPropertyValue>(expected.Value, actual.Value, AssertNumberValue));
        }

        private static void AssertNumberValue(NumberVPropertyValue expected, NumberVPropertyValue actual)
        {
            Assert.Equal(expected.Value, actual.Value);
        }

        private static void AssertBooleanValue(BooleanVPropertyValue expected, BooleanVPropertyValue actual)
        {
            Assert.Equal(expected.Value, actual.Value);
        }

        private static void AssertStringValue(StringVPropertyValue expected, StringVPropertyValue actual)
        {
            Assert.Equal(expected.Value, actual.Value);
        }

        private static void AssertNode(VNode expected, VNode result)
        {
	        GenericAssert.AssertCollection(expected.Children, result.Children, AssertTree);
        }

	    private static void AssertText(VText expected, VText result)
        {
            Assert.Equal(expected.Text, result.Text);
        }
    }
}