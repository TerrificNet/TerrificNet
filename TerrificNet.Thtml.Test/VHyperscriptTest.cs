using System.Collections.Generic;
using System.IO;
using System.Text;
using TerrificNet.Thtml.Hyperscript;
using TerrificNet.Thtml.VDom;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class VHyperscriptTest
    {
        [Theory]
        [MemberData("TestData")]
        public void TestHyperscript(VTree tree, string output)
        {
            var generator = new VHyperscriptRenderer();
            var sb = new StringBuilder();
            var writer = new StringWriter(sb);
            generator.Render(tree, writer);

            Assert.Equal(output, sb.ToString());
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[]
                {
                    new VTree(),
                    string.Empty
                };
                yield return new object[]
                {
                    new VElement("h1"),
                    "h('h1')"
                };
                yield return new object[]
                {
                    new VElement("h1", new[] { new VProperty("attr1", new StringVPropertyValue("val1")), new VProperty("attr2", new StringVPropertyValue("val2")) }),
                    "h('h1',{attr1:'val1',attr2:'val2'})"
                };
                yield return new object[]
                {
                    new VElement("h1", new VElement("h2")),
                    "h('h1',[h('h2')])"
                };
                yield return new object[]
                {
                    new VElement("h1", new VElement("h2"), new VElement("h3")),
                    "h('h1',[h('h2'),h('h3')])"
                };
            }
        }
    }
}
