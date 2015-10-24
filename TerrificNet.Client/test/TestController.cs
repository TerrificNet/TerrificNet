using Microsoft.AspNet.Mvc;

namespace TerrificNet.Client.test
{
    [Route("test")]
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return new ObjectResult(new {Name = "gugus", Text = "text"});
        }
    }
}
