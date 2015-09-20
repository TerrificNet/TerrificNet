using TerrificNet.Thtml.Parsing;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    static internal class NodeAsserts
    {
        public static void AssertNode(CreateNode expected, CreateNode actual)
        {
            if (expected == null)
                Assert.Null(actual);

            Assert.IsType(expected.GetType(), actual);

            var eDocument = expected as CreateDocument;
            var aDocument = actual as CreateDocument;

            var eContent = expected as CreateTextNode;
            var aContent = actual as CreateTextNode;

            var eElement = expected as CreateElement;
            var aElement = actual as CreateElement;

            var eDynamic = expected as DynamicCreateNode;
            var aDynamic = actual as DynamicCreateNode;

            if (eElement != null)
            {
                AssertElement(eElement, aElement);
            }
            if (eDynamic != null)
            {
                AssertDynamic(eDynamic, aDynamic);
            }
            else if (eDocument != null)
            {
                AssertDocument(eDocument, aDocument);
            }
            else if (eContent != null)
            {
                AssertContent(eContent, aContent);
            }
            else
                Assert.True(false, "Unknown type");
        }

        private static void AssertDynamic(DynamicCreateNode eDynamic, DynamicCreateNode aDynamic)
        {
            Assert.Equal(eDynamic.Expression, aDynamic.Expression);
        }

        public static void AssertElement(CreateElement expected, CreateElement actual)
        {
            AssertDocument(expected, actual);
            Assert.Equal(expected.TagName, actual.TagName);

            if (expected.Attributes != null)
            {
                Assert.NotNull(actual.Attributes);
                Assert.Equal(expected.Attributes.Count, actual.Attributes.Count);

                for (int i = 0; i < expected.Attributes.Count; i++)
                {
                    AssertAttribute(expected.Attributes[i], actual.Attributes[i]);
                }
            }
        }

        public static void AssertAttribute(CreateAttribute expected, CreateAttribute actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Value, actual.Value);
        }

        public static void AssertContent(CreateTextNode expected, CreateTextNode actual)
        {
            Assert.Equal(expected.Text, actual.Text);
        }

        public static void AssertDocument(CreateDocument expected, CreateDocument actual)
        {
            var expectedList = expected.ChildNodes;

            Assert.Equal(expectedList.Count, actual.ChildNodes.Count);
            for (int i = 0; i < expectedList.Count; i++)
            {
                AssertNode(expectedList[i], actual.ChildNodes[i]);
            }
        }
    }
}