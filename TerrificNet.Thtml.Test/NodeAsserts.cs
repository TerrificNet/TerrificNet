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

            var eDynamic = expected as Statement;
            var aDynamic = actual as Statement;

            var eDynamicBlock = expected as BlockStatement;
            var aDynamicBlock = actual as BlockStatement;

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

        private static void AssertDynamic(BlockStatement expected, BlockStatement actual)
        {
            HandlebarsExpressionAssert.AssertExpression(expected.Expression, actual.Expression);
            AssertNodeList(expected.ChildNodes, actual.ChildNodes);
        }

        private static void AssertDynamic(Statement expected, Statement actual)
        {
            HandlebarsExpressionAssert.AssertExpression(expected.Expression, actual.Expression);
        }

        public static void AssertElement(Element expected, Element actual)
        {
            AssertDocument(expected, actual);
            Assert.Equal(expected.TagName, actual.TagName);

            AssertElementParts(expected.Attributes, actual.Attributes);
        }

        private static void AssertElementParts(IReadOnlyList<ElementPart> expected, IReadOnlyList<ElementPart> actual)
        {
            if (expected != null)
            {
                Assert.NotNull(actual);
                Assert.Equal(expected.Count, actual.Count);

                for (int i = 0; i < expected.Count; i++)
                {
                    AssertElementPart(expected[i], actual[i]);
                }
            }
        }

        private static void AssertElementPart(ElementPart expected, ElementPart actual)
        {
            if (expected == null)
                Assert.Null(actual);

            var attributeExpected = expected as AttributeNode;
            var dynamicExpected = expected as AttributeStatement;

            if (attributeExpected != null)
            {
                Assert.IsType<AttributeNode>(actual);
                AssertAttribute(attributeExpected, (AttributeNode) actual);
            }
            else if (dynamicExpected != null)
            {
                Assert.IsType<AttributeStatement>(actual);
                AssertExpressionAttribute(dynamicExpected, (AttributeStatement)actual);
            }
            else
                Assert.True(false, "Unknown type");
        }

        private static void AssertExpressionAttribute(AttributeStatement expected, AttributeStatement actual)
        {
            HandlebarsExpressionAssert.AssertExpression(expected.Expression, actual.Expression);
            AssertElementParts(expected.ChildNodes, actual.ChildNodes);
        }

        public static void AssertAttribute(AttributeNode expected, AttributeNode actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            if (expected.Value == null)
                Assert.Null(actual.Value);

            AssertAttributeContent(expected.Value, actual.Value);
        }

        private static void AssertAttributeContent(AttributeContent expected, AttributeContent actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            var eConst = expected as ConstantAttributeContent;
            var eComp = expected as CompositeAttributeContent;
            var eEvaluate = expected as AttributeContentStatement;
            if (eConst != null)
            {
                Assert.IsType<ConstantAttributeContent>(actual);
                Assert.Equal(eConst.Text, ((ConstantAttributeContent) actual)?.Text);
            }
            else if (eComp != null)
            {
                Assert.IsType<CompositeAttributeContent>(actual);
                var aComp = actual as CompositeAttributeContent;

                Assert.Equal(eComp.ContentParts.Length, aComp.ContentParts.Length);
                for (int i = 0; i < eComp.ContentParts.Length; i++)
                {
                    AssertAttributeContent(eComp.ContentParts[i], aComp.ContentParts[i]);
                }
            }
            else if (eEvaluate != null)
            {
                Assert.IsType<AttributeContentStatement>(actual);
                HandlebarsExpressionAssert.AssertExpression(eEvaluate.Expression, ((AttributeContentStatement) actual).Expression);
            }
            else
                throw new Exception("Unexpected type");
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