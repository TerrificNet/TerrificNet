using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using TerrificNet.Models;
using TerrificNet.UnityModules;
using TerrificNet.ViewEngine;
using TerrificNet.ViewEngine.TemplateHandler.UI;

namespace TerrificNet.Controllers
{
    public class DataEditController : AdministrationTemplateControllerBase
    {
		public DataEditController(TerrificNetApplication[] applications) : base(applications)
		{
		}

        [HttpGet]
        public Task<HttpResponseMessage> Index(string schema, string data, string app)
        {
            var model = GetModel(schema, data);
            model.TemplatePartials = string.Join(",", this.ResolveForApp<ITemplateRepository>(app).GetAll().Select(a => string.Concat("\"", a.Id, "\"")).ToArray());

            var viewDefinition = IncludeResources(DefaultLayout.WithDefaultLayout(GetDataEditor(model)));

            AddSaveAction(viewDefinition, model);

            return View(viewDefinition);
        }

        [HttpGet]
        public Task<HttpResponseMessage> IndexAdvanced()
        {
			var model = GetModel(null, null);
            var viewDefinition = IncludeResources(DefaultLayout.WithDefaultLayout(new PartialViewDefinition
            {
                Template = "components/modules/AdvancedEditor/AdvancedEditor",
                ExtensionData = new Dictionary<string, object>
                {
                    { "save_action_id", model.SaveActionId }
                },
                Placeholder = new PlaceholderDefinitionCollection
                {
                    { "rightPanel", new [] { GetDataEditor(model) } }
                }
            }));

            AddSaveAction(viewDefinition, model);

            return View(viewDefinition);
        }

        private static void AddSaveAction(ViewDefinition viewDefinition, DataEditModel model)
        {
            var saveAction = new ActionModel
            {
                Name = "Save",
                Link = "#",
                Id = model.SaveActionId
            };
            viewDefinition.AddAction(saveAction);
        }

        private ViewDefinition GetDataEditor(DataEditModel model)
        {
            return new PartialViewDefinition
            {
                Template = "components/modules/DataEditor/DataEditor",
                Data = model
            };
        }

        private static PageViewDefinition IncludeResources(PageViewDefinition layout)
        {
            return layout
                .IncludeScript("/web/assets/jsoneditor.min.js")
                .IncludeScript("/web/assets/common.js")
				.IncludeScript("/web/assets/dataEditor.js")
                .IncludeStyle("//cdnjs.cloudflare.com/ajax/libs/font-awesome/4.0.3/css/font-awesome.css");
        }

		private DataEditModel GetModel(string schemaUrl, string dataUrl)
        {
            return new DataEditModel
            {
				SchemaUrl = schemaUrl,
				DataUrl = dataUrl,
                SaveActionId = string.Concat("action_", Guid.NewGuid().ToString())
            };
        }

    }
}
