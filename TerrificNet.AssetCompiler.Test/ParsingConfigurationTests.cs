using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using TerrificNet.Test.Common;
using TerrificNet.ViewEngine.Config;
using TerrificNet.ViewEngine.IO;
using Xunit;

namespace TerrificNet.AssetCompiler.Test
{
    
    public class ParsingConfigurationTests
    {
        [Fact]
        public void ParseValidJson()
        {
            const string file = "valid.json";
            var config = ConfigurationLoader.LoadTerrificConfiguration(string.Empty, file, new FileSystem(Path.Combine(PathUtility.GetDirectory(), "configs")));
            Assert.NotNull(config);
            Assert.True(config.Assets.ContainsKey("app.css"));
            Assert.True(config.Assets.ContainsKey("app.js"));
        }

        [Fact]
        public void ParseInvalidJson()
        {
            const string fileName = "invalid.json";
            Assert.Throws<JsonReaderException>(() => ConfigurationLoader.LoadTerrificConfiguration(string.Empty, fileName, new FileSystem(Path.Combine(PathUtility.GetDirectory(), "configs"))));
        }

        [Fact]
        public void ParseDefaultConfig()
        {
            var config = ConfigurationLoader.LoadTerrificConfiguration(string.Empty, new FileSystem(PathUtility.GetFullFilename("configs")));
            Assert.NotNull(config);
            Assert.True(config.Assets.ContainsKey("app.css"));
            Assert.True(config.Assets.ContainsKey("app.js"));
        }

        [Fact]
        public void ParseNullConfig()
        {
            Assert.Throws<ArgumentNullException>(() => ConfigurationLoader.LoadTerrificConfiguration(null, new FileSystem()));
        }
    }
}
