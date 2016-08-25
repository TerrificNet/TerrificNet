using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace TerrificNet.Configuration
{
	public class ServerConfiguration
	{
		[JsonProperty("mimetypes")]
		public IDictionary<string, string> MimeTypes { get; set; }

		public static ServerConfiguration LoadConfiguration(string path)
		{
			using (var reader = new JsonTextReader(new StreamReader(new FileStream(path, FileMode.Open))))
			{
				return new JsonSerializer().Deserialize<ServerConfiguration>(reader);
			}
		}
	}
}
