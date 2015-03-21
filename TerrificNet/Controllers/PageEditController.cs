﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerrificNet.AssetCompiler;
using TerrificNet.AssetCompiler.Helpers;
using TerrificNet.Models;
using TerrificNet.UnityModules;
using TerrificNet.ViewEngine;
using TerrificNet.ViewEngine.Config;
using TerrificNet.ViewEngine.IO;
using TerrificNet.ViewEngine.TemplateHandler;
using Veil;

namespace TerrificNet.Controllers
{
    public class PageEditController : AdministrationTemplateControllerBase
    {
        public PageEditController(TerrificNetApplication[] applications)
            : base(applications)
        {
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Index(string id, string app)
        {
            PageViewDefinition siteDefinition;
            var found = ResolveForApp<TerrificViewDefinitionRepository>(app).TryGetFromViewId(id, out siteDefinition);
            var appViewEnging = ResolveForApp<IViewEngine>(app);
            var tplInfo = await ResolveForApp<ITemplateRepository>(app).GetTemplateAsync(siteDefinition.Template);

            var data = new PageEditModel
            {
                PageJson = found ? JsonConvert.SerializeObject(siteDefinition) : null,
                PageHtml = await CreateSiteHtml(await appViewEnging.CreateViewAsync(tplInfo), siteDefinition),
                Modules = CreateModules(app),
                Layouts = CreateLayouts(app),
                App = app,
                Id = id
            };

            var viewDefinition = DefaultLayout.WithDefaultLayout(new PartialViewDefinition
            {
                Template = "components/modules/PageEditor/PageEditor",
                Data = data
            });
            viewDefinition.IncludeScript("assets/pageEditor.js");
            viewDefinition.IncludeStyle("assets/pageEditor.css");
            viewDefinition.IncludeStyle("/web/page_edit/bundle_app.css?app=" + app);

            return await View(viewDefinition.Template, viewDefinition);
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Save(string id, string app, [FromBody] ReceivedDefinition definition)
        {
            var repo = ResolveForApp<TerrificViewDefinitionRepository>(app);
            var jObject = JsonConvert.DeserializeObject(definition.Definition) as JObject;
            var def = ViewDefinition.FromJObject<PageViewDefinition>(jObject);

            if (await repo.UpdateViewDefinitionForId(id, def))
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetEditorAsset(string name, string app = "")
        {
            var assetHelper = ResolveForApp<IAssetHelper>(app);
            var assetBundler = ResolveForApp<IAssetBundler>(app);
            var assetCompilerFactory = ResolveForApp<IAssetCompilerFactory>(app);
            var config = ResolveForApp<ITerrificNetConfig>(app);
            var fileSystem = ResolveForApp<IFileSystem>(app);

            var components = assetHelper.GetGlobComponentsForAsset(config.Assets[name], fileSystem.BasePath);
            var content = await assetBundler.BundleAsync(components);
            content = ".page-editor .page, .page-editor .sidebar{" + content + "}";
            var compiler = assetCompilerFactory.GetCompiler(name);
            var compiledContent = await compiler.CompileAsync(content);

            var response = new HttpResponseMessage
            {
                Content = new StringContent(compiledContent, Encoding.Default, compiler.MimeType)
            };
            return response;
        }

        [HttpGet]
        public async Task<ElementEditorDefinition> GetModuleDefinition(string id, string parent, string skin = null,
            string dataVariation = null, string app = "")
        {
            var renderer = ResolveForApp<ITerrificTemplateHandlerFactory>(app).Create();

            var htmlBuilder = new StringBuilder();
            using (var writer = new StringWriter(htmlBuilder))
            {
                var context = new RenderingContext(writer);
                context.Data.Add("pageEditor", true);
                context.Data.Add("data_variation", dataVariation);
                context.Data.Add("renderPath", new List<string> {parent});
                context.Data.Add("siteDefinition", new ModuleViewDefinition());
                context.Data.Add("short_module", true);
                await renderer.RenderModuleAsync(id, skin, context);

                var moduleViewDefinition = context.Data["siteDefinition"] as ModuleViewDefinition;
                return new ElementEditorDefinition
                {
                    Html = htmlBuilder.ToString(),
                    Placeholder = moduleViewDefinition != null ? moduleViewDefinition.Placeholder : null
                };
            }
        }

        [HttpGet]
        public async Task<ElementEditorDefinition> GetLayoutDefinition(string id, string parent, string app = "")
        {
            var renderer = ResolveForApp<ITerrificTemplateHandlerFactory>(app).Create();

            var htmlBuilder = new StringBuilder();
            using (var writer = new StringWriter(htmlBuilder))
            {
                var context = new RenderingContext(writer);
                context.Data.Add("pageEditor", true);
                context.Data.Add("renderPath", new List<string> { parent });
                context.Data.Add("siteDefinition", new PartialViewDefinition());
                context.Data.Add("layoutPlaceholders", new PlaceholderDefinitionCollection());
                await renderer.RenderPartialAsync(id, null, context);

                var layoutViewDefinition = context.Data["siteDefinition"] as PartialViewDefinition;
                return new ElementEditorDefinition
                {
                    Html = htmlBuilder.ToString(),
                    Placeholder = layoutViewDefinition != null ? layoutViewDefinition.Placeholder : null
                };
            }
        }

        private static async Task<string> CreateSiteHtml(IView view, PageViewDefinition siteDefinition)
        {
            var pageHtmlBuilder = new StringBuilder();
            using (var writer = new StringWriter(pageHtmlBuilder))
            {
                var context = new RenderingContext(writer);
                context.Data.Add("pageEditor", true);
	            context.Data.Add("siteDefinition", siteDefinition);

                await view.RenderAsync(JObject.FromObject(siteDefinition), context).ConfigureAwait(false);
            }

            var html = pageHtmlBuilder.ToString();

            html = html.Replace("<!DOCTYPE html>", "").Replace("<html>", "").Replace("</html>", "");
            html = new Regex("<head>(.*)</head>", RegexOptions.Singleline).Replace(html, "");
            html = new Regex("<script( src[=]\".*\")?>(.*)</script>", RegexOptions.Singleline).Replace(html, "");

            return html;
        }

        private IEnumerable<PageEditModuleModel> CreateModules(string app)
        {
            var modelRepository = ResolveForApp<IModuleRepository>(app);
            var modelProvider = ResolveForApp<IModelProvider>(app);
            var replacePath = ResolveForApp<ITerrificNetConfig>(app).ModulePath;
            if (!replacePath.EndsWith("/")) replacePath += "/";
            var models = (from mod in modelRepository.GetAll()
                let skins = mod.Skins.Select(s => s.Key).ToList()
                select new PageEditModuleModel
            {
                    Id = mod.Id,
                    Name = mod.Id.Replace(replacePath, ""),
                    Skins = skins,
                    Variations = modelProvider.GetDataVariations(mod).ToList().Aggregate("", (s, s1) => s += s1 + "|", s => s.Length > 0 ? s.Substring(0, s.Length-1) : s)
                }).ToList();
            return models.Count > 0 ? models : null;
        }

        private IEnumerable<PageEditLayoutModel> CreateLayouts(string app)
                {
            var templateRepo = ResolveForApp<ITemplateRepository>(app);
            var config = ResolveForApp<ITerrificNetConfig>(app);

            var replacePath = config.ViewPath;
            if (!replacePath.EndsWith("/")) replacePath += "/";

            var tpls = templateRepo.GetAll()
                .Where(t => t.Id.StartsWith(config.ViewPath))
                .Select(tpl => new PageEditLayoutModel
                    {
                    Id = tpl.Id,
                    Name = tpl.Id.Replace(replacePath, "")
                })
                .ToList();
            return tpls.Count > 0 ? tpls : null;
                }

        public class ReceivedDefinition
                {
            [JsonProperty("definition")]
            public string Definition { get; set; }
        }
    }
}