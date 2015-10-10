using System;
using System.Collections.Generic;
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

            AssertOneOf(
                () => AssertValue<VText>(expected, result, AssertText),
                () => AssertValue<VElement>(expected, result, AssertElement),
                () => AssertValue<VNode>(expected, result, AssertNode));
        }

        private static void AssertOneOf(params Func<bool>[] assertions)
        {
            foreach (var assert in assertions)
            {
                if (assert())
                    return;
            }
            Assert.False(true, "No Type found");
        }

        private static void AssertElement(VElement expected, VElement result)
        {
            Assert.Equal(expected.TagName, result.TagName);
            AssertCollection(expected.Children, result.Children, AssertTree);
            AssertCollection(expected.Properties, result.Properties, AssertProperty);
        }

        private static void AssertProperty(VProperty expected, VProperty actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            AssertOneOf(
                () => AssertValue<StringVPropertyValue>(expected.Value, actual.Value, AssertStringValue),
                () => AssertValue<BooleanVPropertyValue>(expected.Value, actual.Value, AssertBooleanValue),
                () => AssertValue<NumberVPropertyValue>(expected.Value, actual.Value, AssertNumberValue));
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
            AssertCollection(expected.Children, result.Children, AssertTree);
        }

        private static void AssertCollection<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual, Action<T, T> assertion)
        {
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                assertion(expected[0], actual[0]);
            }
        }

        private static void AssertText(VText expected, VText result)
        {
            Assert.Equal(expected.Text, result.Text);
        }

        private static bool AssertValue<T>(object expected, object result, Action<T, T> assertion)
            where T : class
        {
            var t = expected as T;
            if (t != null)
            {
                Assert.IsType<T>(result);
                assertion(t, result as T);

                return true;
            }
            return false;
        }
    }
}