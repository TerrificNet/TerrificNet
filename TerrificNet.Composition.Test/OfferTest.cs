using System;
using TerrificNet.Composition.Test.Mocks;
using Xunit;

namespace TerrificNet.Composition.Test
{
    public class OfferTest
    {
        [Fact]
        public void Offer_Create_FailsOnInferface()
        {
            Assert.Throws<ArgumentException>(() => Offer.FromType(typeof(ITestInterface1)));
        }

        [Fact]
        public void Offer_Create_AcceptsConcreteType()
        {
            var offer = Offer.FromType(typeof(Impl11));
            Assert.NotNull(offer);
            Assert.Equal(typeof(Impl11), offer.Type);
        }

    }
}
