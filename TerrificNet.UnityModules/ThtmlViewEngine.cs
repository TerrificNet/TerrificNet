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
        public Task<IView> CreateViewAsync(TemplateInfo templateInfo, Type modelType, IModelBinder modelBinder)
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
            var dataBinder = TypeDataBinder.BinderFromType(modelType);
            var method = compiler.Emit(ast, dataBinder, null);

            return Task.FromResult<IView>(new ThtmlView(method));
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
}
