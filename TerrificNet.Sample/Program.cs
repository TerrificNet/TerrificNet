using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace TerrificNet.Sample
{
    public class Program
    {
		public static void Main(string[] args)
		{
			new ConfigurationBuilder().AddJsonFile("asdf");
			var host = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<Startup>()
				.Build();

			host.Run();
		}
	}
}
