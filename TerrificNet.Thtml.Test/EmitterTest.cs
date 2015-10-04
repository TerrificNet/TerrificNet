using System.Collections.Generic;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class EmitterTest
    {
        [Theory]
        [MemberData("TestData")]
        public void TestEmit(string description, Document input, VTree expected)
        {
            var compiler = new Emitter();
            var method = compiler.Emit(input);

            var result = method();

            VTreeAsserts.AssertTree(expected, result);
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[]
                {
                    "empty document",
                    new Document(),
                    new VNode()
                };

                yield return new object[]
                {
                    "one element",
                    new Document(
                        new Element("h1", 
                            new TextNode("hallo"))),
                    new VNode(
                        new VElement("h1", 
                            new VText("hallo")))
                };
            }
        }
    }
}