﻿using System.IO;
using Microsoft.Practices.Unity;
using TerrificNet.Environment;
using TerrificNet.Generator;
using TerrificNet.ViewEngine;
using TerrificNet.ViewEngine.Cache;
using TerrificNet.ViewEngine.Client;
using TerrificNet.ViewEngine.Config;
using TerrificNet.ViewEngine.Globalization;
using TerrificNet.ViewEngine.IO;
using TerrificNet.ViewEngine.ModelProviders;
using TerrificNet.ViewEngine.SchemaProviders;
using TerrificNet.ViewEngine.TemplateHandler;
using TerrificNet.ViewEngine.ViewEngines;
using Veil.Compiler;
using Veil.Helper;

namespace TerrificNet.UnityModules
{
    public class DefaultUnityModule : IUnityModule
    {
        public void Configure(IUnityContainer container)
        {
            container.RegisterType<IModuleRepository, DefaultModuleRepository>();
            container.RegisterType<IModuleSchemaProvider, DefaultModuleSchemaProvider>();
            container.RegisterType<IHelperHandlerFactory, DefaultRenderingHelperHandlerFactory>();
            container.RegisterType<IMemberLocator, MemberLocatorFromNamingRule>();
        }

        public static TerrificNetApplication RegisterForApplication(IUnityContainer childContainer, string hostPath, string basePath, string applicationName, string section)
        {
            try
            {
                var fileSystem = childContainer.Resolve<FileSystemProvider>().GetFileSystem(hostPath, basePath, out basePath);
                childContainer.RegisterInstance(fileSystem);

                var config = ConfigurationLoader.LoadTerrificConfiguration(basePath, fileSystem);
                var application = new TerrificNetApplication(applicationName, section, config, childContainer);

                var projectPath = PathInfo.Create("terrific.json");
                if (fileSystem.FileExists(projectPath))
                {
                    var project = Project.FromFile(new StreamReader(fileSystem.OpenRead(projectPath)).ReadToEnd(), fileSystem);
                    childContainer.RegisterInstance(project);
                }

                childContainer.RegisterInstance(application);
                RegisterForConfiguration(childContainer, config);

                return application;
            }
            catch (ConfigurationException ex)
            {
                throw new InvalidApplicationException(string.Format("Could not load the configuration for application '{0}'.", applicationName), ex);
            }
        }

        public static void RegisterForConfiguration(IUnityContainer container, ITerrificNetConfig item)
        {
            container.RegisterInstance(item);
            RegisterApplicationSpecific(container);
        }

        private static void RegisterApplicationSpecific(IUnityContainer container)
        {
            container.RegisterType<IViewEngine, VeilViewEngine>();
            container.RegisterType<ICacheProvider, MemoryCacheProvider>();
            container.RegisterType<IModelProvider, JsonModelProvider>();
			//container.RegisterType<IModelTypeProvider, DynamicModelTypeProvider>();

            container.RegisterType<ISchemaProvider, SchemaMergeProvider>(
                new InjectionConstructor(new ResolvedParameter<HandlebarsViewSchemaProvider>(),
                    new ResolvedParameter<FileSystemSchemaProvider>()));
            container.RegisterType<ISchemaProviderFactory, UnitySchemaProviderFactory>();
            container.RegisterType<IJsonSchemaCodeGenerator, JsonSchemaCodeGenerator>();
            container.RegisterType<IModuleRepository, DefaultModuleRepository>();
	        container.RegisterType<IClientTemplateGenerator, ClientTemplateGenerator>();
			container.RegisterType<IClientTemplateGeneratorFactory, UnityClientGeneratorFactory>();
            container.RegisterType<ITemplateRepository, TerrificTemplateRepository>(new ContainerControlledLifetimeManager());

	        container.RegisterType<ILabelService, JsonLabelService>();
        }

		private class UnityClientGeneratorFactory : IClientTemplateGeneratorFactory
		{
			private readonly IUnityContainer _container;

			public UnityClientGeneratorFactory(IUnityContainer container)
			{
				_container = container;
			}

			public IClientTemplateGenerator Create()
			{
				return _container.Resolve<IClientTemplateGenerator>();
			}
		}

	    private class UnitySchemaProviderFactory : ISchemaProviderFactory
        {
            private readonly IUnityContainer _container;

            public UnitySchemaProviderFactory(IUnityContainer container)
            {
                _container = container;
            }

            public ISchemaProvider Create()
            {
                return _container.Resolve<ISchemaProvider>();
            }
        }
    }
}
