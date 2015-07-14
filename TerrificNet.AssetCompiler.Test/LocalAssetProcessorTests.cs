using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using TerrificNet.AssetCompiler.Bundler;
using TerrificNet.AssetCompiler.Compiler;
using TerrificNet.AssetCompiler.Helpers;
using TerrificNet.AssetCompiler.Processors;
using TerrificNet.Test.Common;
using TerrificNet.ViewEngine.Config;
using TerrificNet.ViewEngine.IO;
using Xunit;

namespace TerrificNet.AssetCompiler.Test
{
	
	public class LocalAssetProcessorTests
	{
        private readonly ITerrificNetConfig _terrificConfig;
		private readonly UnityContainer _container;

        public LocalAssetProcessorTests()
		{
            _terrificConfig = ConfigurationLoader.LoadTerrificConfiguration("configs", new FileSystem(PathUtility.GetDirectory()));

		    _container = new UnityContainer();
			_container.RegisterType<IAssetCompiler, JsAssetCompiler>("Js");
			_container.RegisterType<IAssetCompiler, LessAssetCompiler>("Css");
			_container.RegisterType<IAssetCompilerFactory, AssetCompilerFactory>();
			_container.RegisterType<IAssetBundler, DefaultAssetBundler>();
			_container.RegisterType<IAssetHelper, AssetHelper>();
			_container.RegisterType<IAssetProcessor, BuildAssetProcessor>();
		}

		[Fact]
		public async Task BundleJsTest()
		{
			var bundler = _container.Resolve<IAssetBundler>();
			var helper = _container.Resolve<IAssetHelper>();
			var components = helper.GetGlobComponentsForAsset(_terrificConfig.Assets["app.js"], "");
			var bundle = await bundler.BundleAsync(components).ConfigureAwait(false);
			Assert.True(bundle.Contains("TestLongParamName"));
		}

		[Fact]
        public async Task BundleCssTest()
		{
			var bundler = _container.Resolve<IAssetBundler>();
			var helper = _container.Resolve<IAssetHelper>();
			var components = helper.GetGlobComponentsForAsset(_terrificConfig.Assets["app.css"], "");
			var bundle = await bundler.BundleAsync(components).ConfigureAwait(false);
			Assert.True(bundle.Contains(".example-css"));
		}

		[Fact]
		public async Task CompileAppJsAssetTest()
		{
			var factory = _container.Resolve<IAssetCompilerFactory>();
			var compiler = factory.GetCompiler("app.js");

			var helper = _container.Resolve<IAssetHelper>();
			var components = helper.GetGlobComponentsForAsset(_terrificConfig.Assets["app.js"], "");
			var bundle = await new DefaultAssetBundler().BundleAsync(components).ConfigureAwait(false);
			var compile = await compiler.CompileAsync(bundle, true).ConfigureAwait(false);
			Assert.False(compile.Contains("TestLongParamName"));
		}

		[Fact]
		public async Task CompileAppCssAssetTest()
		{
			var factory = _container.Resolve<IAssetCompilerFactory>();
			var compiler = factory.GetCompiler("app.css");

			var helper = _container.Resolve<IAssetHelper>();
			var components = helper.GetGlobComponentsForAsset(_terrificConfig.Assets["app.css"], "");
			var bundle = await new DefaultAssetBundler().BundleAsync(components).ConfigureAwait(false);
			var compile = await compiler.CompileAsync(bundle, true).ConfigureAwait(false);
			Assert.True(compile.Contains(".mod-example{background:#000}"));
		}

		[Fact]
		public void AssetCompilerFactoryJsTest()
		{
			var factory = _container.Resolve<IAssetCompilerFactory>();

			var compiler = factory.GetCompiler("app.js");
            Assert.IsType(typeof(JsAssetCompiler), compiler);
		}

		[Fact]
		public void AssetCompilerFactoryCssTest()
		{
			var factory = _container.Resolve<IAssetCompilerFactory>();

			var compiler = factory.GetCompiler("app.css");
            Assert.IsType(typeof(LessAssetCompiler), compiler);
		}

		[Fact]
		public async Task BuildAssetProcessJsWithoutMinifyTest()
		{
			var processor = _container.Resolve<IAssetProcessor>();
			var asset = _terrificConfig.Assets.First(o => o.Key == "app.js");
			var processed = await processor.ProcessAsync(asset.Key, asset.Value, ProcessorFlags.None, "").ConfigureAwait(false);
			Assert.True(processed.Contains("TestLongParamName"));
		}

		[Fact]
		public async Task BuildAssetProcessCssWithoutMinifyTest()
		{
			var processor = _container.Resolve<IAssetProcessor>();
			var asset = _terrificConfig.Assets.First(o => o.Key == "app.css");
			var processed = await processor.ProcessAsync(asset.Key, asset.Value, ProcessorFlags.None, "").ConfigureAwait(false);
			Assert.True(processed.Contains(".example-css"));
		}

		[Fact]
		public async Task BuildAssetProcessJsWithMinifyTest()
		{
			var processor = _container.Resolve<IAssetProcessor>();
			var asset = _terrificConfig.Assets.First(o => o.Key == "app.js");
			var processed = await processor.ProcessAsync(asset.Key, asset.Value, ProcessorFlags.Minify, "").ConfigureAwait(false);
			Assert.False(processed.Contains("TestLongParamName"));
		}

		[Fact]
		public async Task BuildAssetProcessCssWithMinifyTest()
		{
			var processor = _container.Resolve<IAssetProcessor>();
			var asset = _terrificConfig.Assets.First(o => o.Key == "app.css");
			var processed = await processor.ProcessAsync(asset.Key, asset.Value, ProcessorFlags.Minify, "").ConfigureAwait(false);
			Assert.True(processed.Contains(".mod-example{background:#000}"));
		}
	}
}
