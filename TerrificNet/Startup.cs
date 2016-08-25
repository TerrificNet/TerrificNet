using Microsoft.AspNetCore.Builder;

namespace TerrificNet
{
	public class Startup
	{
		public void Configuration(IApplicationBuilder container)
		{
			container.UseMvc(rb =>
			{
				rb.MapRoute(
					name: "AdministrationHome",
					template: "web/",
					defaults: new {controller = "Home", action = "Index", section = "web/"}
					);

				rb.MapRoute(
					name: "AdministrationModuleDetail",
					template: "web/module/{action}",
					defaults: new {controller = "ModuleDetail", action = "Index", section = "web/"}
					);

				rb.MapRoute(
					name: "Project",
					template: "web/project/{app}/",
					defaults: new {controller = "Project", action = "Index", section = "web/"}
					);

				rb.MapRoute(
					name: "ProjectItems",
					template: "web/project/{app}/{projectKind}/{*id}",
					defaults: new {controller = "ProjectItem", action = "Index", section = "web/"}
					);


				rb.MapRoute(
					name: "AdministrationDataEdit",
					template: "web/edit",
					defaults: new {controller = "DataEdit", action = "Index", section = "web/"}
					);

				rb.MapRoute(
					name: "AdministrationDataEditAdvanced",
					template: "web/edit_advanced",
					defaults: new {controller = "DataEdit", action = "IndexAdvanced", section = "web/"}
					);

				rb.MapRoute(
					name: "PageEditor",
					template: "web/page_edit",
					defaults: new {controller = "PageEdit", section = "web/"}
					);

				rb.MapRoute(
					name: "PageEditorSiteBundles",
					template: "web/page_edit/bundle_{name}",
					defaults: new {controller = "PageEdit", action = "GetEditorAsset", section = "web/"}
					);

				rb.MapRoute(
					name: "PageEditorModuleInfo",
					template: "web/page_edit/element_info/module",
					defaults: new {controller = "PageEdit", action = "GetModuleDefinition", section = "web/"}
					);

				rb.MapRoute(
					name: "PageEditorLayoutInfo",
					template: "web/page_edit/element_info/layout",
					defaults: new {controller = "PageEdit", action = "GetLayoutDefinition", section = "web/"}
					);

				rb.MapRoute(
					name: "CoreFiles",
					template: "$tcn/{*path}",
					defaults: new {controller = "staticfile"}
					);
			});
			//foreach (var application in container.ResolveAll<TerrificNetApplication>())
			//{
			//	var section = application.Section;

			//	MapArea(config, application.Container, section);
			//}
		}

		//private static void MapArea(HttpConfiguration config, IUnityContainer container, string section = null)
		//{
		//	rb.MapRoute(
		//		name: "ModelRoot" + section,
		//		template: section + "model/{*path}",
		//		defaults: new { controller = "model", section = section }
		//		);
		//	rb.MapRoute(
		//		name: "ModuleSchemaRoot" + section,
		//		template: section + "module_schema/{*path}",
		//		defaults: new { controller = "moduleschema", section = section }
		//		);
		//	rb.MapRoute(
		//		name: "SchemaRoot" + section,
		//		template: section + "schema/{*path}",
		//		defaults: new { controller = "schema", section = section }
		//		);
		//	rb.MapRoute(
		//		name: "GenerateRoot" + section,
		//		template: section + "generate/{*path}",
		//		defaults: new { controller = "generate", section = section }
		//		);
		//	rb.MapRoute(
		//		name: "ClientRoot" + section,
		//		template: section + "js/{*path}",
		//		defaults: new { controller = "clienttemplate", section = section }
		//		);
		//	rb.MapRoute(
		//		name: "AssetsRoot" + section,
		//		template: section + "assets/{*path}",
		//		defaults: new { controller = "assets", section = section }
		//		);
		//	rb.MapRoute(
		//		name: "BundleRoot" + section,
		//		template: section + "bundle_{name}",
		//		defaults: new { controller = "bundle", section = section }
		//		);
		//	rb.MapRoute(
		//		name: section + "TemplateRoot",
		//		template: section + "{*path}",
		//		defaults: new { controller = "template", section = section },
		//		constraints: new { path = container.Resolve<ValidTemplateRouteConstraint>() }
		//		);
		//	rb.MapRoute(
		//		name: section + "TemplateRootDefault",
		//		template: section,
		//		defaults: new { controller = "template", path = "index", section = section },
		//		constraints: new { path = container.Resolve<ValidTemplateRouteConstraint>() }
		//		);
		//	rb.MapRoute(
		//		name: section + "StaticFile",
		//		template: section + "{*path}",
		//		defaults: new { controller = "staticfile", section = section }
		//		);
		//}
	}
}