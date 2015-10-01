using System;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.VDom;

namespace TerrificNet.Thtml.Compiler
{
    public class Compiler
    {
        public Func<VTree> Compile(Document input)
        {
            return () => new VTree();
        }
    }
}