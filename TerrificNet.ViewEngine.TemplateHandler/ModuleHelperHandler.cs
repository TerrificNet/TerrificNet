using System;
using System.Collections.Generic;

namespace TerrificNet.ViewEngine.TemplateHandler
{
    internal class ModuleHelperHandler : IHelperHandler
    {
        private readonly ITerrificTemplateHandler _handler;

        public ModuleHelperHandler(ITerrificTemplateHandler handler)
        {
            _handler = handler;
        }

        public bool IsSupported(string name)
        {
            return name.StartsWith("module", StringComparison.OrdinalIgnoreCase);
        }

		public void Evaluate(object model, RenderingContext context, IDictionary<string, string> parameters)
		{
			var templateName = parameters["template"].Trim('"');

			var skin = string.Empty;
			if (parameters.ContainsKey("skin"))
				skin = parameters["skin"].Trim('"');

			_handler.RenderModule(templateName, skin, context);
		}
    }
}