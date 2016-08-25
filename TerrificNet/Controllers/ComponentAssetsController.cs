using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Configuration;
using TerrificNet.ViewEngine.Config;
using TerrificNet.ViewEngine.IO;

namespace TerrificNet.Controllers
{
	public class ComponentAssetsController : StaticFileController
	{
	    private readonly PathInfo _filePath;

	    public ComponentAssetsController(ITerrificNetConfig config, IFileSystem fileSystem, ServerConfiguration serverConfiguration) 
			: base(fileSystem, serverConfiguration)
	    {
	        _filePath = config.ModulePath;
	    }

	    [HttpGet]
		public override HttpResponseMessage Get(string path)
		{
			return GetInternal(path);
		}

        protected override PathInfo FilePath
		{
			get { return _filePath; }
		}
	}
}