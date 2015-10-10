using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private readonly ITemplateRepository _templateRepository;

        public ThtmlViewEngine(ITemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        public Task<IView> CreateViewAsync(TemplateInfo templateInfo, Type modelType, IModelBinder modelBinder)
        {
            var emitter = CreateEmitter(templateInfo, TypeDataBinder.BinderFromType(modelType), new BasicHelperBinder(_templateRepository));

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
        private readonly ITemplateRepository _templateRepository;

        public BasicHelperBinder(ITemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
        {
            if ("partial".Equals(helper, StringComparison.InvariantCultureIgnoreCase))
                return new PartialHelperBinderResult(_templateRepository, arguments["template"]);

            return null;
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
