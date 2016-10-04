using Microsoft.AspNetCore.Mvc;
using TerrificNet.Mvc.Core;

namespace TerrificNet.Sample.Controllers
{
	public class HomePageController : ControllerBase
	{
		[HttpGet]
		public IActionResult Index()
		{
			return new ViewResult("Index", new
			{
				title = "Start",
				list = new[] { "s1", "s2" }
			});
		}
	}
}
