using System.Collections.Generic;
using TerrificNet.ViewEngine.TemplateHandler.Grid;

namespace TerrificNet.ViewEngine.TemplateHandler
{
    public class DefaultRenderingHelperHandlerFactory : IHelperBinder
   {
        private readonly ITerrificTemplateHandlerFactory _terrificTemplateHandlerFactory;
        private readonly ITemplateRepository _templateRepository;
        private readonly ISchemaProviderFactory _schemaProviderFactory;
		private readonly IClientTemplateGeneratorFactory _clientTemplateGeneratorFactory;

	    public DefaultRenderingHelperHandlerFactory(
            ITerrificTemplateHandlerFactory terrificTemplateHandlerFactory,
            ITemplateRepository templateRepository,
            ISchemaProviderFactory schemaProviderFactory,
			IClientTemplateGeneratorFactory clientTemplateGeneratorFactory)
        {
            _terrificTemplateHandlerFactory = terrificTemplateHandlerFactory;
            _templateRepository = templateRepository;
            _schemaProviderFactory = schemaProviderFactory;
	        _clientTemplateGeneratorFactory = clientTemplateGeneratorFactory;
        }

        public IEnumerable<IHelperHandler> Create()
        {
            yield return new ModuleHelperHandler(_terrificTemplateHandlerFactory.Create());
            yield return new PartialHelperHandler(_terrificTemplateHandlerFactory.Create(), _schemaProviderFactory.Create(), _templateRepository, _clientTemplateGeneratorFactory.Create());
            yield return new PlaceholderHelperHandler(_terrificTemplateHandlerFactory.Create());
            yield return new LabelHelperHandler(_terrificTemplateHandlerFactory.Create());
            yield return new GridHelperHandler();
            yield return new GridWidthHelperHandler();
			yield return new GridComponentWidthHelperHandler();
            yield return new TemplateIdHelperHandler();
        }
    }
}