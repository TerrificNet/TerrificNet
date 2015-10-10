using System;
using System.Collections.Generic;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.Test.Asserts;
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

                var obj2 = new
                {
                    Items = new[] { new Dummy { Name = "test1" }, new Dummy { Name = "test2" } }
                };
                yield return new object[]
                {
                    "one element with iteration expression",
                    new Document(
                        new Element("h1",
                            new Statement(
                                new IterationExpression(new MemberExpression("items")), 
                            new Element("div", 
                                new Statement(new MemberExpression("name")))))),
                    TypeDataBinder.BinderFromObject(obj2),
                    obj2,
                    new VNode(
                        new VElement("h1",
                            new VElement("div",
                                new VText("test1")),
                            new VElement("div",
                                new VText("test2"))))
                };
                var obj3 = new { Name = "value" };
                yield return new object[]
                {
                    "one element with attribute expression",
                    new Document(
                        new Element("h1", new ElementPart[] { new AttributeNode("title", new AttributeContentStatement(new MemberExpression("name"))) })),
                    TypeDataBinder.BinderFromObject(obj3),
                    obj3,
                    new VNode(
                        new VElement("h1", new[] { new VProperty("title", new StringVPropertyValue("value")) }, null))
                };
            }
        }
    }

    public class Dummy
    {
        public string Name { get; set; }
    }

    public class NullDataBinder : IDataBinder
    {
        public DataBinderResult Evaluate(string propertyName)
        {
            return null;
        }

        public DataBinderResult Item()
        {
            return null;
        }

        public DataBinderResult Context()
        {
            return null;
        }
    }
}