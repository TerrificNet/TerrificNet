using Xunit;

namespace TerrificNet.Thtml.Test
{
    static internal class NodeAsserts
    {
        public static void AssertNode(HtmlNode expected, HtmlNode actual)
        {
            if (expected == null)
                Assert.Null(actual);

            Assert.IsType(expected.GetType(), actual);

            var eDocument = expected as HtmlDocument;
            var aDocument = actual as HtmlDocument;

            var eContent = expected as HtmlTextNode;
            var aContent = actual as HtmlTextNode;

            var eElement = expected as HtmlElement;
            var aElement = actual as HtmlElement;

            var eDynamic = expected as DynamicHtmlNode;
            var aDynamic = actual as DynamicHtmlNode;

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

        private static void AssertDynamic(DynamicHtmlNode eDynamic, DynamicHtmlNode aDynamic)
        {
            Assert.Equal(eDynamic.Expression, aDynamic.Expression);
        }

        public static void AssertElement(HtmlElement expected, HtmlElement actual)
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

        public static void AssertAttribute(HtmlAttribute expected, HtmlAttribute actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Value, actual.Value);
        }

        public static void AssertContent(HtmlTextNode expected, HtmlTextNode actual)
        {
            Assert.Equal(expected.Text, actual.Text);
        }

        public static void AssertDocument(HtmlDocument expected, HtmlDocument actual)
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