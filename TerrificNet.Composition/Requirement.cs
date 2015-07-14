using System;
using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Composition
{
    public class Requirement
    {
        private readonly Type _type;

        public Requirement(Type type)
        {
            _type = type;
        }

        public static Requirement ImplementationFromType(Type type)
        {
            return new Requirement(type);
        }

        public IEnumerable<Offer> GetMatchingOffers(List<Offer> offers)
        {
            return offers.Where(Match);
        }

        private bool Match(Offer offer)
        {
            return offer.GetImplementingInferfaces().Contains(_type);
        }
    }
}