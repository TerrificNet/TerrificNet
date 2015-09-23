using System;
using System.Collections.Generic;
using TerrificNet.Thtml.Parsing;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    static internal class NodeAsserts
    {
        public static void AssertNode(Node expected, Node actual)
        {
            if (expected == null)
                Assert.Null(actual);

            Assert.IsType(expected.GetType(), actual);

            var eDocument = expected as Document;
            var aDocument = actual as Document;

            var eContent = expected as TextNode;
            var aContent = actual as TextNode;

            var eElement = expected as Element;
            var aElement = actual as Element;

            var eDynamic = expected as EvaluateExpressionNode;
            var aDynamic = actual as EvaluateExpressionNode;

            var eDynamicBlock = expected as EvaluateBlockNode;
            var aDynamicBlock = actual as EvaluateBlockNode;

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
            else if (eDynamicBlock != null)
            {
                AssertDynamic(eDynamicBlock, aDynamicBlock);
            }
            else
                Assert.True(false, "Unknown type");
        }

        private static void AssertDynamic(EvaluateBlockNode expected, EvaluateBlockNode actual)
        {
            Assert.Equal(expected.Expression, actual.Expression);
            AssertNodeList(expected.ChildNodes, actual.ChildNodes);
        }

        private static void AssertDynamic(EvaluateExpressionNode expected, EvaluateExpressionNode actual)
        {
            Assert.Equal(expected.Expression, actual.Expression);
        }

        public static void AssertElement(Element expected, Element actual)
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

        public static void AssertAttribute(AttributeNode expected, AttributeNode actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Value, actual.Value);
        }

        public static void AssertContent(TextNode expected, TextNode actual)
        {
            Assert.Equal(expected.Text, actual.Text);
        }

        public static void AssertDocument(Document expected, Document actual)
        {
            AssertNodeList(expected.ChildNodes, actual.ChildNodes);
        }

        private static void AssertNodeList(IReadOnlyList<Node> expectedList, IReadOnlyList<Node> acutalList)
        {
            Assert.Equal(expectedList.Count, acutalList.Count);
            for (int i = 0; i < expectedList.Count; i++)
            {
                AssertNode(expectedList[i], acutalList[i]);
            }
        }
    }
}