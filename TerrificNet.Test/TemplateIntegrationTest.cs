using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using TerrificNet.Generator;
using TerrificNet.Test.Common;
using TerrificNet.ViewEngine;
using TerrificNet.ViewEngine.Cache;
using TerrificNet.ViewEngine.Client;
using TerrificNet.ViewEngine.Client.Javascript;
using TerrificNet.ViewEngine.Client.Test;
using TerrificNet.ViewEngine.IO;
using TerrificNet.ViewEngine.SchemaProviders;
using TerrificNet.ViewEngine.ViewEngines;
using Veil;
using Veil.Helper;
using Xunit;

namespace TerrificNet.Test
{
    public class TemplateIntegrationTest
    {
        [Theory]
        [MemberData("TestData")]
        public async Task TestServerSideRendering(string testName, string templateFile, string dataFile, string resultFile)
        {
            var resultString = await ExecuteServerSide(testName, templateFile, dataFile).ConfigureAwait(false);
            Assert.Equal(GetFileContent(resultFile), resultString);
        }

        [Theory]
        [MemberData("TestData")]
        public async Task TestServerSideRenderingWithStronglyTypedModel(string testName, string templateFile, string dataFile, string resultFile)
        {
            var resultString = await ExecuteServerSideStrongModel(testName, templateFile, dataFile).ConfigureAwait(false);
            Assert.Equal(GetFileContent(resultFile), resultString);
        }

        [Theory]
        [MemberData("TestData")]
        public void TestClientSideRenderingWithJavascript(string testName, string templateFile, string dataFile, string resultFile)
        {
            var resultString = ExecuteClientSide(testName, templateFile, dataFile);
            Assert.Equal(GetFileContent(resultFile), resultString);
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                string content = GetFileContent("TestCases/test_cases.csv");
                return content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Select(l =>
                {
                    var objs = l.Split(',').Select(s => Path.Combine("TestCases", s)).ToArray();
                    return new[] {Path.GetFileName(Path.GetDirectoryName(objs[2]))}.Union(objs).ToArray();
                });
            }
        }

        private static string GetFileContent(string resultFile)
        {
            string result;
            using (var reader = new StreamReader(PathUtility.GetFullFilename(resultFile)))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        private string ExecuteClientSide(string testName, string templateFile, string dataFile)
        {
            var namingRule = new NamingRule();
            var handlerFactory = new NullRenderingHelperHandlerFactory();

            var clientGenerator = new ClientTemplateGenerator(handlerFactory, new MemberLocatorFromNamingRule(namingRule));
            var generator = new JavascriptClientTemplateGenerator("repo", clientGenerator);

            var templateInfo = new FileTemplateInfo(testName, PathInfo.Create(templateFile), new FileSystem());
            var view = generator.Generate(templateInfo);

            Dictionary<string, object> model;
            using (var reader = new StreamReader(dataFile))
            {
                model = (Dictionary<string, object>) new JavaScriptSerializer().Deserialize(reader.ReadToEnd(), typeof(Dictionary<string, object>));
            }

            CleanupModel(model);

            var result = JavascriptClientTest.ExecuteJavascript(view, model, testName);
            return result;
        }

        private class StringWriterDelayed : StringWriter
        {
            public StringWriterDelayed(StringBuilder sb) : base(sb)
            {
            }

            public override async Task WriteAsync(string value)
            {
                await Task.Yield();
                //await Task.Delay(100);
                await base.WriteAsync(value);
            }

            public override async Task WriteAsync(char value)
            {
                await Task.Yield();
                //await Task.Delay(100);
                await base.WriteAsync(value);
            }
        }

        private void CleanupModel(Dictionary<string, object> model)
        {
            foreach (var entry in model.ToList())
            {
                var value = entry.Value;
                var list = value as ArrayList;
                if (list != null)
                {
                    var objs = list.OfType<object>().ToList();
                    model[entry.Key] = objs;
                    value = objs;

                    //continue;
                }

                var enumerable = value as IEnumerable<object>;
                if (enumerable != null)
                {
                    foreach (var item in enumerable)
                    {
                        var dItem = item as Dictionary<string, object>;
                        if (dItem != null)
                            CleanupModel(dItem);
                    }
                }

                var dict = value as Dictionary<string, object>;
                if (dict != null)
                    CleanupModel(dict);
            }
        }

        private class MyResolver : JavaScriptTypeResolver
        {
            public MyResolver()
            {
            }

            public override Type ResolveType(string id)
            {
                return typeof (Dictionary<string, object>);
            }

            public override string ResolveTypeId(Type type)
            {
                return type.Name;
            }
        }

        private async Task<string> ExecuteServerSideStrongModel(string testName, string templateFile, string dataFile)
        {
            var cacheProvider = new NullCacheProvider();
            var namingRule = new NamingRule();
            var handlerFactory = new NullRenderingHelperHandlerFactory();

            var templateInfo = new FileTemplateInfo(testName, PathInfo.Create(templateFile), new FileSystem());

            var schemaProvider = new HandlebarsViewSchemaProvider(handlerFactory, new MemberLocatorFromNamingRule(namingRule));

            var generator = new JsonSchemaCodeGenerator(namingRule);
            var schema = await schemaProvider.GetSchemaFromTemplateAsync(templateInfo).ConfigureAwait(false);
            schema.Title = "Model";
            var modelType = generator.Compile(schema);

            var viewEngine = new VeilViewEngine(cacheProvider, handlerFactory, namingRule);
            var view = await viewEngine.CreateViewAsync(templateInfo, modelType).ConfigureAwait(false);
            if (view == null)
                Assert.True(false, string.Format("Could not create view from file '{0}'.", templateFile));

            object model;
            using (var reader = new JsonTextReader(new StreamReader(dataFile)))
            {
                model = JsonSerializer.Create().Deserialize(reader, modelType);
            }

            var builder = new StringBuilder();
            using (var writer = new StringWriterDelayed(builder))
            {
                view.Render(model, new RenderingContext(writer));
            }
            var resultString = builder.ToString();
            return resultString;
        }

        private async Task<string> ExecuteServerSide(string testName, string templateFile, string dataFile)
        {
            var cacheProvider = new NullCacheProvider();
            var namingRule = new NamingRule();
            var handlerFactory = new NullRenderingHelperHandlerFactory();

            var templateInfo = new FileTemplateInfo(testName, PathInfo.Create(templateFile), new FileSystem());

            var viewEngine = new VeilViewEngine(cacheProvider, handlerFactory, namingRule);
            IView view = await viewEngine.CreateViewAsync(templateInfo).ConfigureAwait(false);
            if (view == null)
                Assert.True(false, string.Format("Could not create view from file'{0}'.", templateFile));

            object model;
            using (var reader = new StreamReader(dataFile))
            {
                model = new JsonSerializer().Deserialize(reader, typeof(Dictionary<string, object>));
            }

            var builder = new StringBuilder();
            using (var writer = new StringWriterDelayed(builder))
            {
                view.Render(model, new RenderingContext(writer));
            }
            var resultString = builder.ToString();
            return resultString;
        }

        private class NullRenderingHelperHandlerFactory : IHelperHandlerFactory
        {
            public IEnumerable<IHelperHandler> Create()
            {
                return Enumerable.Empty<IHelperHandler>();
            }
        }
    }
}
