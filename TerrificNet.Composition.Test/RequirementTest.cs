using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Composition.Test.Mocks;
using Xunit;


namespace TerrificNet.Composition.Test
{
    
    public class RequirementTest
    {
        [Fact]
        public void Requirement_GetMatchingOffers_ReturnsAllFromSubtype()
        {
            var match = Offer.FromType(typeof(Impl21));
            var offers = new List<Offer>
            {
                Offer.FromType(typeof(Impl11)),
                Offer.FromType(typeof(Impl12)),
                match
            };

            var underTest = Requirement.ImplementationFromType(typeof(ITestInterface2));

            var result = underTest.GetMatchingOffers(offers);
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(1, resultList.Count);
            Assert.Equal(match, resultList[0]);

        }

    }
}
