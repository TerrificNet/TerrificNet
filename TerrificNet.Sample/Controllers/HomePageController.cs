using Microsoft.AspNetCore.Mvc;

namespace TerrificNet.Sample.Controllers
{
	public class HomePageController : ControllerBase
	{
		[HttpGet]
		public IActionResult Index()
		{
			return new ViewResult("asdf", new { title = "Start", body = "asdf", list = new[] { "s1", "s2" } });
		}
	}
}
