using Microsoft.AspNetCore.Mvc;

namespace Reusable.Apps.Server.Controllers
{
    //[ApiVersion("1.0")]
    [Route("[controller]")]
    public class HomeController : Controller
    {

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Welcome to Vault!");
        }
    }
}

