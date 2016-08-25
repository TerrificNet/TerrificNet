using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using TerrificNet.Configuration;
using TerrificNet.ViewEngine;
using TerrificNet.ViewEngine.IO;

namespace TerrificNet.Controllers
{
    public class StaticFileController : Controller
	{
        private readonly IFileSystem _fileSystem;
	    private readonly ServerConfiguration _serverConfiguration;

	    public StaticFileController(IFileSystem fileSystem, ServerConfiguration serverConfiguration)
        {
	        _fileSystem = fileSystem;
	        _serverConfiguration = serverConfiguration;
        }

	    public virtual HttpResponseMessage Get(string path)
	    {
		    return GetInternal(path);
	    }

        protected virtual PathInfo FilePath { get { return null; } }

	    protected HttpResponseMessage GetInternal(string path)
	    {
	        var pathInfo = PathInfo.Create(path);

	        var filePath = _fileSystem.Path.Combine(this.FilePath, pathInfo);
            if (!_fileSystem.FileExists(filePath))
                return new HttpResponseMessage(HttpStatusCode.NotFound);

	        var eTag = new EntityTagHeaderValue(string.Concat("\"", _fileSystem.GetFileInfo(filePath).Etag, "\""));
	        if (Request.Headers["IfNoneMatch"].Count > 0)
	        {
	            foreach (var noneMatch in Request.Headers["IfNoneMatch"])
	            {
	                if (eTag.Equals(noneMatch))
	                    return new HttpResponseMessage(HttpStatusCode.NotModified);
	            }
	        }

            var stream = _fileSystem.OpenRead(filePath);
            var message = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
            message.Content.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(_fileSystem.Path.GetExtension(pathInfo).ToLower()));
	        message.Headers.ETag = eTag;
            return message;
	    }

		private string GetMimeType(string extension)
		{
			if (extension == null)
			{
				throw new ArgumentNullException(nameof(extension));
			}

			if (!extension.StartsWith("."))
			{
				extension = "." + extension;
			}

			string mime;
			return _serverConfiguration.MimeTypes.TryGetValue(extension, out mime) ? mime : "application/octet-stream";
		}
	}
}