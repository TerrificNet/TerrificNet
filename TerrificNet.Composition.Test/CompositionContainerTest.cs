using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using TerrificNet.Composition.Test.Mocks;
using Xunit;


namespace TerrificNet.Composition.Test
{
    
    public class CompositionContainerTest
    {
        [Fact]
        public void CompositionContainer_RegisterModule_IsInList()
        {
            var underTest = new CompositionContainer();
            var module = new Mock<IModule>().Object;

            underTest.RegisterModule(module);

            Assert.True(underTest.GetModules().Contains(module), "registered module should be enlisted.");
        }

        [Fact]
        public void CompositionContainer_RegisterExisitingModule_ThrowsException()
        {
            var underTest = new CompositionContainer();
            var module = new Mock<IModule>().Object;

            underTest.RegisterModule(module);

            Assert.Throws<ArgumentException>(() => underTest.RegisterModule(module));
        }

        [Fact]
        public void CompositionContainer_GetRequirements_GetsAllRequirementsFromModules()
        {
            var underTest = new CompositionContainer();
            var mod1 = new Mock<IModule>();
            var mod2 = new Mock<IModule>();

            mod1.Setup(r => r.GetRequirements()).Returns(new List<Requirement> { Requirement.ImplementationFromType(typeof(string)), Requirement.ImplementationFromType(typeof(int)) });
            mod2.Setup(r => r.GetRequirements()).Returns(new List<Requirement> { Requirement.ImplementationFromType(typeof(double)) });

            underTest.RegisterModule(mod1.Object);
            underTest.RegisterModule(mod2.Object);

            var result = underTest.GetRequirements();
            Assert.NotNull(result);

            var list = result.ToList();
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void CompositionContainer_GetOffers_GetsAllOffersFromModules()
        {
            var underTest = new CompositionContainer();
            var mod1 = new Mock<IModule>();
            var mod2 = new Mock<IModule>();

            mod1.Setup(r => r.GetOffers()).Returns(new List<Offer> { Offer.FromType(typeof(string)), Offer.FromType(typeof(int)) });
            mod2.Setup(r => r.GetOffers()).Returns(new List<Offer> { Offer.FromType(typeof(double)) });

            underTest.RegisterModule(mod1.Object);
            underTest.RegisterModule(mod2.Object);

            var result = underTest.GetOffers();
            Assert.NotNull(result);

            var list = result.ToList();
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void CompositionContainer_VerifyCorrectConfiguration_Verified()
        {
            var underTest = new CompositionContainer();
            var mod1 = CreateModule(new List<Offer> { Offer.FromType(typeof(Impl11)) }, null);
            var mod2 = CreateModule(null, new List<Requirement> { Requirement.ImplementationFromType(typeof(ITestInterface1)) });
            underTest.RegisterModule(mod1);
            underTest.RegisterModule(mod2);

            var result = underTest.Verify();
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public void CompositionContainer_VerifyMissingConfiguration_ReturnsMissing()
        {
            var underTest = new CompositionContainer();
            var mod1 = CreateModule(new List<Offer> {Offer.FromType(typeof (Impl21))}, null);
            var req1 = Requirement.ImplementationFromType(typeof(ITestInterface1));
            var mod2 = CreateModule(null, new List<Requirement> { req1 });

            underTest.RegisterModule(mod1);
            underTest.RegisterModule(mod2);

            var result = underTest.Verify();
            Assert.NotNull(result);
            Assert.False(result.Success);

            var missingList = result.GetMissingRequirements().ToList();
            Assert.Equal(1, missingList.Count);
            Assert.Equal(req1, missingList[0]);
        }

        [Fact]
        public void CompositionContainer_VerifyNotUniqueConfiguration_ReturnsNotUniqueConfiguration()
        {
            var underTest = new CompositionContainer();
            var offerMod1 = Offer.FromType(typeof(Impl11));
            var mod1 = CreateModule(new List<Offer> { offerMod1 }, null);
            var req1 = Requirement.ImplementationFromType(typeof(ITestInterface1));
            var offerMod2 = Offer.FromType(typeof(Impl12));
            var mod2 = CreateModule(new List<Offer> { offerMod2 }, new List<Requirement> { req1 });

            underTest.RegisterModule(mod1);
            underTest.RegisterModule(mod2);

            var result = underTest.Verify();
            Assert.NotNull(result);
            Assert.False(result.Success);

            var notMatching = result.GetNotMatchingRequirements().ToList();
            Assert.Equal(1, notMatching.Count);
            Assert.NotNull(notMatching[0]);
            Assert.Equal(req1, notMatching[0].Requirement);
            Assert.NotNull(notMatching[0].Offers);
            Assert.Equal(2, notMatching[0].Offers.Count);
            Assert.Equal(offerMod1, notMatching[0].Offers[0]);
            Assert.Equal(offerMod2, notMatching[0].Offers[1]);
        }

        private static IModule CreateModule(IEnumerable<Offer> offers, IEnumerable<Requirement> requirements)
        {
            var mod1 = new Mock<IModule>();
            mod1.Setup(m => m.GetOffers()).Returns(offers);
            mod1.Setup(m => m.GetRequirements()).Returns(requirements);
            return mod1.Object;
        }

    }

}
