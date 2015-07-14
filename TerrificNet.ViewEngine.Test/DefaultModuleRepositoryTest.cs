using System.Linq;
using Moq;
using TerrificNet.ViewEngine.Config;
using TerrificNet.ViewEngine.IO;
using Xunit;

namespace TerrificNet.ViewEngine.Test
{
    
    public class DefaultModuleRepositoryTest
    {
        [Fact]
        public void TestOnlyUseTemplatesFromModulePath()
        {
            var templateRepository = CreateRepository("modules/Mod1/Mod1", "modules/Mod2/Mod2", "layouts/Layout1");
            var terrificNetConfig = CreateConfig();

            var underTest = new DefaultModuleRepository(terrificNetConfig, templateRepository);
            var result = underTest.GetAll().ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("modules/Mod1", result[0].Id);
            Assert.Equal("modules/Mod2", result[1].Id);
        }

        [Fact]
        public void TestModuleContainsSkins()
        {
            var templateRepository = CreateRepository("modules/Mod1/Mod1-skin1", "modules/Mod1/Mod1-skin2");
            var terrificNetConfig = CreateConfig();

            var underTest = new DefaultModuleRepository(terrificNetConfig, templateRepository);
            var result = underTest.GetAll().ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal("modules/Mod1", result[0].Id);
            Assert.Equal(2, result[0].Skins.Count);
            Assert.True(result[0].Skins.ContainsKey("skin1"), "Expected to have skin with name skin1");
            Assert.True(result[0].Skins.ContainsKey("skin2"), "Expected to have skin with name skin2");
        }

        [Fact]
        public void TestModuleContainsUseDefaultTemplateWithSameName()
        {
            var templateRepository = CreateRepository("modules/Mod1/Mod1", "modules/Mod1/Mod1-skin1", "modules/Mod1/Mod1-skin2");
            var terrificNetConfig = CreateConfig();

            var underTest = new DefaultModuleRepository(terrificNetConfig, templateRepository);
            var result = underTest.GetAll().ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal("modules/Mod1", result[0].Id);
            Assert.NotNull(result[0].DefaultTemplate);
            Assert.Equal("modules/Mod1/Mod1", result[0].DefaultTemplate.Id);
            Assert.Equal(2, result[0].Skins.Count);
        }

        [Fact]
        public void TestModuleContainsUseDefaultTemplateWhenOnlyOneTemplate()
        {
            var templateRepository = CreateRepository("modules/Mod1/test");
            var terrificNetConfig = CreateConfig();

            var underTest = new DefaultModuleRepository(terrificNetConfig, templateRepository);
            var result = underTest.GetAll().ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal("modules/Mod1", result[0].Id);
            Assert.NotNull(result[0].DefaultTemplate);
            Assert.Equal("modules/Mod1/test", result[0].DefaultTemplate.Id);
            Assert.Equal(0, result[0].Skins.Count);
        }

        [Fact]
        public void TestModuleContainsUseDefaultTemplateWhenOnlyOneTemplateWithoutSkin()
        {
            var templateRepository = CreateRepository("modules/Mod1/test", "modules/Mod1/test-skin");
            var terrificNetConfig = CreateConfig();

            var underTest = new DefaultModuleRepository(terrificNetConfig, templateRepository);
            var result = underTest.GetAll().ToList();

            Assert.Equal(1, result.Count);
            Assert.Equal("modules/Mod1", result[0].Id);
            Assert.NotNull(result[0].DefaultTemplate);
            Assert.Equal("modules/Mod1/test", result[0].DefaultTemplate.Id);
            Assert.Equal(1, result[0].Skins.Count);
        }


        private static ITerrificNetConfig CreateConfig()
        {
            var config = new Mock<ITerrificNetConfig>();
            config.Setup(c => c.ModulePath).Returns(PathInfo.Create("modules"));

            return config.Object;
        }

        private static ITemplateRepository CreateRepository(params string[] templates)
        {
            var templateRepo = new Mock<ITemplateRepository>();
            templateRepo.Setup(t => t.GetAll()).Returns(templates.Select(id => new StringTemplateInfo(id, "")));
            return templateRepo.Object;
        }
    }
}
