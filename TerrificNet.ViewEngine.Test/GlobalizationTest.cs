using TerrificNet.ViewEngine.Globalization;
using TerrificNet.ViewEngine.IO;
using Xunit;

namespace TerrificNet.ViewEngine.Test
{
	
	public class GlobalizationTest
	{
		private const string TestKey = "name";
		private const string TestPathKey = "person/male";

		private readonly JsonLabelService _service;
		private readonly FileSystem _fileSystem;

		public GlobalizationTest()
		{
			_fileSystem = new FileSystem();
			_service = new JsonLabelService(_fileSystem);
			_service.Remove(TestKey);
			_service.Remove(TestPathKey);
		}

		[Fact]
		public void TestRoot()
		{
			_service.Get(TestKey);
			_service.Set(TestKey, "xyz");

			var name = _service.Get(TestKey);
			Assert.Equal("xyz", name);

			_service.Remove(TestKey);
		}

		[Fact]
		public void TestPath()
		{
			_service.Get(TestPathKey);

			_service.Set(TestPathKey, "xyz");

			var name = _service.Get(TestPathKey);
			Assert.Equal("xyz", name);

			_service.Remove(TestPathKey);
		}

		[Fact]
		public void TestReload()
		{
			_service.Set(TestPathKey, "xyz");

			var service2 = new JsonLabelService(_fileSystem);
			var name = service2.Get(TestPathKey);
			Assert.Equal("xyz", name);
		}
	}
}
