using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.LexicalAnalysis;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;
using TerrificNet.Thtml.VDom;
using TerrificNet.ViewEngine;
using Veil;

namespace TerrificNet.UnityModules
{
    public class ThtmlViewEngine : IViewEngine
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ITemplateRepository _templateRepository;

        public ThtmlViewEngine(IModuleRepository moduleRepository, ITemplateRepository templateRepository)
        {
            _moduleRepository = moduleRepository;
            _templateRepository = templateRepository;
        }

        public Task<IView> CreateViewAsync(TemplateInfo templateInfo, Type modelType, IModelBinder modelBinder)
        {
            IDataBinder binder;
            if (modelType == typeof(object))
                binder = new DynamicDataBinder();
            else
                binder = TypeDataBinder.BinderFromType(modelType);

            var emitter = CreateEmitter(templateInfo, binder, new BasicHelperBinder(_moduleRepository, _templateRepository));

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
                var result = _method.Execute(new ObjectDataContext(model));
                context.Writer.Write(result);
            }
        }
    }

    public class BasicHelperBinder : IHelperBinder
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ITemplateRepository _templateRepository;

        public BasicHelperBinder(IModuleRepository moduleRepository, ITemplateRepository templateRepository)
        {
            _moduleRepository = moduleRepository;
            _templateRepository = templateRepository;
        }

        public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
        {
            if ("partial".Equals(helper, StringComparison.InvariantCultureIgnoreCase))
                return new PartialHelperBinderResult(_templateRepository, arguments["template"]);

            if ("module".Equals(helper, StringComparison.InvariantCultureIgnoreCase))
                return new ModuleHelperBinderResult(_moduleRepository, arguments["template"], arguments.ContainsKey("skin") ? arguments["skin"] : null);

            if ("placeholder".Equals(helper, StringComparison.InvariantCultureIgnoreCase))
                return new PlaceholderHelperBinderResult();

            return null;
        }
    }

    public class PlaceholderHelperBinderResult : HelperBinderResult
    {
        public override IListEmitter<T> CreateEmitter<T>(IListEmitter<T> children, IHelperBinder helperBinder, IDataBinder scope)
        {
            return children;
        }
    }

    public class ModuleHelperBinderResult : HelperBinderResult
    {
        private readonly IModuleRepository _templateRepository;
        private readonly string _module;
        private readonly string _skin;

        public ModuleHelperBinderResult(IModuleRepository templateRepository, string module, string skin)
        {
            _templateRepository = templateRepository;
            _module = module;
            _skin = skin;
        }

        public override IListEmitter<T> CreateEmitter<T>(IListEmitter<T> children, IHelperBinder helperBinder, IDataBinder scope)
        {
            var moduleDescription = _templateRepository.GetModuleDefinitionByIdAsync(_module).Result;
            var template = moduleDescription.DefaultTemplate;

            return EmitterNode.AsList((IEmitter<T>)ThtmlViewEngine.CreateEmitter(template, scope, helperBinder));
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
