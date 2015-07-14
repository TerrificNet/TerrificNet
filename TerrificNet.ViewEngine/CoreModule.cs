using System;
using System.Collections.Generic;
using TerrificNet.Composition;
using TerrificNet.ViewEngine.IO;
using TerrificNet.ViewEngine.SchemaProviders;

namespace TerrificNet.ViewEngine
{
    public class CoreModule : IModule
    {
        public IEnumerable<Requirement> GetRequirements()
        {
            yield return Requirement.ImplementationFromType(typeof(IModuleRepository));
            yield return Requirement.ImplementationFromType(typeof(IModuleSchemaProvider));
            yield return Requirement.ImplementationFromType(typeof(ITemplateRepository));
            yield return Requirement.ImplementationFromType(typeof(IModelTypeProvider));
            yield return Requirement.ImplementationFromType(typeof(IFileSystem));
            yield return Requirement.ImplementationFromType(typeof(ISchemaProvider));
        }

        public IEnumerable<Offer> GetOffers()
        {
            yield return Offer.FromType(typeof(DefaultModuleRepository));
            yield return Offer.FromType(typeof(DefaultModuleSchemaProvider));
            yield return Offer.FromType(typeof(TerrificTemplateRepository));
            yield return Offer.FromType(typeof(StaticModelTypeProvider));
        }
    }
}
