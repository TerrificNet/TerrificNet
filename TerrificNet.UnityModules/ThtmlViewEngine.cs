using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;
using TerrificNet.ViewEngine;
using TerrificNet.ViewEngine.TemplateHandler.UI;
using Veil;

namespace TerrificNet.UnityModules
{
    public class ThtmlViewEngine : IViewEngine
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IModelProvider _modelProvider;

        public ThtmlViewEngine(IModuleRepository moduleRepository, ITemplateRepository templateRepository, IModelProvider modelProvider)
        {
            _moduleRepository = moduleRepository;
            _templateRepository = templateRepository;
            _modelProvider = modelProvider;
        }

        public Task<IView> CreateViewAsync(TemplateInfo templateInfo, Type modelType, IModelBinder modelBinder)
        {
            IDataBinder binder;
            if (modelType == typeof(object))
                binder = new DynamicDataBinder();
            else
                binder = TypeDataBinder.BinderFromType(modelType);

            var emitter = CreateEmitter(templateInfo, binder, new BasicHelperBinder(_moduleRepository, _templateRepository, _modelProvider));

            return Task.FromResult<IView>(new ThtmlView(emitter));
        }

        internal static IEmitter<VTree> CreateEmitter(TemplateInfo templateInfo, IDataBinder dataBinder, IHelperBinder helperBinder)
        {
            string template;
            using (var reader = new StreamReader(templateInfo.Open()))
            {
                template = reader.ReadToEnd();
            }

            var lexer = new Lexer();
            var tokens = lexer.Tokenize(template);
            var parser = new Parser(new HandlebarsParser());
            var ast = parser.Parse(tokens);
            var compiler = new Emitter();

            var emitter = compiler.Emit(ast, dataBinder, helperBinder);
            return emitter;
        }

        private class ThtmlView : IView
        {
            private readonly IEmitter<VTree> _method;

            public ThtmlView(IEmitter<VTree> method)
            {
                _method = method;
            }

            public void Render(object model, RenderingContext context)
            {
                var result = _method.Execute(new ObjectDataContext(model), new RenderingContextAdapter(context));
                context.Writer.Write(result);
            }

            private class RenderingContextAdapter : IRenderingContext
            {
                private readonly RenderingContext _renderingContext;

                public RenderingContextAdapter(RenderingContext renderingContext)
                {
                    _renderingContext = renderingContext;
                }

                public bool TryGetData<T>(string key, out T obj)
                {
                    return _renderingContext.TryGetData(key, out obj);
                }
            }
        }
    }

    public class BasicHelperBinder : IHelperBinder
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IModelProvider _modelProvider;

        public BasicHelperBinder(IModuleRepository moduleRepository, ITemplateRepository templateRepository, IModelProvider modelProvider)
        {
            _moduleRepository = moduleRepository;
            _templateRepository = templateRepository;
            _modelProvider = modelProvider;
        }

        public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
        {
            if ("partial".Equals(helper, StringComparison.InvariantCultureIgnoreCase))
                return new PartialHelperBinderResult(_templateRepository, arguments["template"]);

            if ("module".Equals(helper, StringComparison.InvariantCultureIgnoreCase))
                return new ModuleHelperBinderResult(_moduleRepository, _modelProvider, arguments["template"], arguments.ContainsKey("skin") ? arguments["skin"] : null);

            if ("placeholder".Equals(helper, StringComparison.InvariantCultureIgnoreCase))
                return new PlaceholderHelperBinderResult(arguments["key"], _moduleRepository, _modelProvider);

            return null;
        }
    }

    public class PlaceholderHelperBinderResult : HelperBinderResult
    {
        private readonly string _key;
        private readonly IModuleRepository _moduleRepository;
        private readonly IModelProvider _modelProvider;

        public PlaceholderHelperBinderResult(string key, IModuleRepository moduleRepository, IModelProvider modelProvider)
        {
            _key = key;
            _moduleRepository = moduleRepository;
            _modelProvider = modelProvider;
        }

        public override IListEmitter<T> CreateEmitter<T>(IListEmitter<T> children, IHelperBinder helperBinder, IDataBinder scope)
        {
            return new PlaceholderEmitter<T>(_key, helperBinder, _moduleRepository, _modelProvider);
        }

        private class PlaceholderEmitter<T> : IListEmitter<T>
        {
            private readonly string _name;
            private readonly IHelperBinder _helperBinder;
            private readonly IModuleRepository _moduleRepository;
            private readonly IModelProvider _modelProvider;

            public PlaceholderEmitter(string name, IHelperBinder helperBinder, IModuleRepository moduleRepository, IModelProvider modelProvider)
            {
                _name = name;
                _helperBinder = helperBinder;
                _moduleRepository = moduleRepository;
                _modelProvider = modelProvider;
            }

            public IEnumerable<T> Execute(IDataContext context, IRenderingContext renderingContext)
            {
                ViewDefinition definition;
                if (!renderingContext.TryGetData("siteDefinition", out definition))
                    throw new InvalidOperationException("The context must contain a siteDefinition to use the placeholder helper.");

                if (definition.Placeholder == null)
                    return Enumerable.Empty<T>();

                ViewDefinition[] definitions;
                if (!definition.Placeholder.TryGetValue(_name, out definitions))
                    return Enumerable.Empty<T>();

                return EmitterNode.AsList(GetEmitters(definitions)).Execute(context, renderingContext);
            }

            private IEnumerable<IEmitter<T>> GetEmitters(IEnumerable<ViewDefinition> definitions)
            {
                foreach (var placeholderConfig in definitions.OfType<ModuleViewDefinition>())
                {
                    yield return ModuleHelperBinderResult.CreateModuleEmitter<T>(_helperBinder, _moduleRepository, _modelProvider,
                        placeholderConfig.Module);
                    // TODO: Move to view definition
                    //var ctx = new RenderingContext(context.Writer, context);
                    //ctx.Data["siteDefinition"] = placeholderConfig;


                    //placeholderConfig.Render(this, model, ctx);
                }
            }
        }
    }

    public class ModuleHelperBinderResult : HelperBinderResult
    {
        private readonly IModuleRepository _templateRepository;
        private readonly IModelProvider _modelProvider;
        private readonly string _module;
        private readonly string _skin;

        public ModuleHelperBinderResult(IModuleRepository templateRepository, IModelProvider modelProvider, string module, string skin)
        {
            _templateRepository = templateRepository;
            _modelProvider = modelProvider;
            _module = module;
            _skin = skin;
        }

        public override IListEmitter<T> CreateEmitter<T>(IListEmitter<T> children, IHelperBinder helperBinder, IDataBinder scope)
        {
            var moduleEmitter = CreateModuleEmitter<T>(helperBinder, _templateRepository, _modelProvider, _module);
            return EmitterNode.AsList(moduleEmitter);
        }

        internal static IEmitter<T> CreateModuleEmitter<T>(IHelperBinder helperBinder, IModuleRepository templateRepository, IModelProvider modelProvider, string module)
        {
            var moduleDescription = templateRepository.GetModuleDefinitionByIdAsync(module).Result;
            var data = modelProvider.GetModelForModuleAsync(moduleDescription, null).Result;
            var template = moduleDescription.DefaultTemplate;

            var context = new ObjectDataContext(data);
            var binder = new DynamicDataBinder();

            var emitter = ThtmlViewEngine.CreateEmitter(template, binder, helperBinder);
            var moduleEmitter = new ModuleEmitter<T>((IEmitter<T>) emitter, context);
            return moduleEmitter;
        }

        private class ModuleEmitter<T> : IEmitter<T>
        {
            private readonly IEmitter<T> _adaptee;
            private readonly IDataContext _dataContext;

            public ModuleEmitter(IEmitter<T> adaptee, IDataContext dataContext)
            {
                _adaptee = adaptee;
                _dataContext = dataContext;
            }

            public T Execute(IDataContext context, IRenderingContext renderingContext)
            {
                return _adaptee.Execute(_dataContext, renderingContext);
            }
        }
    }

    public class PartialHelperBinderResult : HelperBinderResult
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly string _templateName;

        public PartialHelperBinderResult(ITemplateRepository templateRepository, string templateName)
        {
            _templateRepository = templateRepository;
            _templateName = templateName;
        }

        public override IListEmitter<T> CreateEmitter<T>(IListEmitter<T> listEmitter, IHelperBinder helperBinder, IDataBinder scope)
        {
            var template = _templateRepository.GetTemplateAsync(_templateName).Result;

            return EmitterNode.AsList((IEmitter<T>)ThtmlViewEngine.CreateEmitter(template, scope, helperBinder));
        }
    }
}
