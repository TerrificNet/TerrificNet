using System;
using System.Collections.Generic;
using TerrificNet.Thtml.VDom;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class VTreeAsserts
    {
        public static void AssertTree(VTree expected, VTree result)
        {
            if (expected == null)
            {
                Assert.Null(result);
                return;
            }

            AssertOneOf(
                () => AssertTree<VText>(expected, result, AssertText),
                () => AssertTree<VElement>(expected, result, AssertElement),
                () => AssertTree<VNode>(expected, result, AssertNode));
        }

        private static void AssertOneOf(params Func<bool>[] assertions)
        {
            foreach (var assert in assertions)
            {
                if (assert())
                    return;
            }
        }

        private static void AssertElement(VElement expected, VElement result)
        {
            Assert.Equal(expected.TagName, result.TagName);
            AssertCollection(expected.Children, result.Children, AssertTree);
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

        private static bool AssertTree<T>(VTree expected, VTree result, Action<T, T> assertion)
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