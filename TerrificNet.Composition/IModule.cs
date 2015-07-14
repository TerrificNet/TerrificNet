using System.Collections.Generic;

namespace TerrificNet.Composition
{
    public interface IModule
    {
        IEnumerable<Requirement> GetRequirements();
        IEnumerable<Offer> GetOffers();
    }
}