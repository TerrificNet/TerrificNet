using System.Collections.Generic;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class CompilerTest
    {
        [Theory]
        [MemberData("TestData")]
        public void TestCompiler(Document input, VTree expected)
        {
            var compiler = new Compiler.Compiler();
            var method = compiler.Compile(input);

            var result = method();

            VTreeAsserts.Assert(expected, result);
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new object[]
                {
                    new Document(),
                    new VTree()
                };
            }
        }
    }
}