using System;
using System.IO;
using Microsoft.Practices.Unity;
using TerrificNet.Configuration;
using TerrificNet.Environment;
using TerrificNet.UnityModules;
using TerrificNet.ViewEngine;
using TerrificNet.ViewEngine.IO;
using TerrificNet.ViewEngine.TemplateHandler;

namespace TerrificNet
{
	static public class WebInitializer
	{
		public static UnityContainer Initialize(string path)
		{
			var configuration = TerrificNetHostConfigurationLoader.LoadConfiguration(Path.Combine(path, "application.json"));
			var serverConfiguration = ServerConfiguration.LoadConfiguration(Path.Combine(path, "server.json"));

			return Initialize(path, configuration, serverConfiguration);
		}

		public static UnityContainer Initialize(string path, TerrificNetHostConfiguration configuration, ServerConfiguration serverConfiguration)
		{
			var container = new UnityContainer();
		    container.RegisterType<ITerrificTemplateHandler, DefaultTerrificTemplateHandler>();
			container 
				.RegisterType
				//<ITerrificTemplateHandlerFactory, GenericUnityTerrificTemplateHandlerFactory<DefaultTerrificTemplateHandler>>();
				<ITerrificTemplateHandlerFactory, GenericUnityTerrificTemplateHandlerFactory<PageEditDefaultTerrificTemplateHandler>>();
			container.RegisterType<INamingRule, NamingRule>();
			container.RegisterInstance(serverConfiguration);

			new DefaultUnityModule().Configure(container);

#if DEBUG
		    var fileSystem = new FileSystem(path);
		    container.RegisterInstance<IFileSystem>(fileSystem);
#else
			container.RegisterInstance<IFileSystem>(new EmbeddedResourceFileSystem(typeof(WebInitializer).Assembly));
#endif

            foreach (var item in configuration.Applications.Values)
			{
				var childContainer = container.CreateChildContainer();

				var app = DefaultUnityModule.RegisterForApplication(childContainer, path, item.BasePath,
					item.ApplicationName, item.Section);
				container.RegisterInstance(item.ApplicationName, app);
			}

			foreach (var app in container.ResolveAll<TerrificNetApplication>())
			{
				foreach (var template in app.Container.Resolve<ITemplateRepository>().GetAll())
				{
					Console.WriteLine(template.Id);
				}
			}

			new TerrificBundleUnityModule().Configure(container);
			return container;
		}
	}
}