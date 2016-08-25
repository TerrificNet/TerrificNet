using System.Collections.Generic;
using Newtonsoft.Json;

namespace TerrificNet.ViewEngine.TemplateHandler.UI
{
    public abstract class ViewDefinition
    {
        [JsonProperty("_placeholder")]
        public PlaceholderDefinitionCollection Placeholder { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> ExtensionData { get; set; }

		protected internal abstract void Render(ITerrificTemplateHandler templateHandler, object model, RenderingContext context);
	}
}