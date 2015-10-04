using System;
using System.Collections.Generic;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class EmitterTest
    {
        [Theory]
        [MemberData("TestData")]
        public void TestEmit(string description, Document input, IDataBinder dataBinder, object data, VTree expected)
        {
            var compiler = new Emitter();
            var method = compiler.Emit(input, dataBinder);

            var result = method.Execute(new ObjectDataContext(data));

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
                    new NullDataBinder(),
                    null,
                    new VNode()
                };

                yield return new object[]
                {
                    "one element",
                    new Document(
                        new Element("h1", 
                            new TextNode("hallo"))),
                    new NullDataBinder(),
                    null,
                    new VNode(
                        new VElement("h1", 
                            new VText("hallo")))
                };
                var obj = new { Name = "hallo" };   
                yield return new object[]
                {
                    "one element with dynamic expression",
                    new Document(
                        new Element("h1",
                            new Statement(new MemberExpression("name")))),
                    TypeDataBinder.BinderFromObject(obj),
                    obj,
                    new VNode(
                        new VElement("h1",
                            new VText("hallo")))
                };
            }
        }
    }

    public class ObjectDataContext : IDataContext
    {
        public ObjectDataContext(object value)
        {
            this.Value = value;
        }

        public object Value { get; }
    }

    public class NullDataBinder : IDataBinder
    {
        public Func<IDataContext, string> Evaluate(MemberExpression memberExpression)
        {
            return d => null;
        }
    }
}