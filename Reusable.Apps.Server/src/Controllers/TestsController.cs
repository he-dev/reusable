using Microsoft.AspNetCore.Mvc;

namespace Reusable.Apps.Server.Controllers
{
    //[ApiVersion("1.0")]
    [Route("/api/[controller]")]
    public class TestsController : Controller
    {
        [HttpGet]
        public IActionResult Get(string message)
        {
            return Ok("Hallo GET!");
        }

        [HttpPut]
        public IActionResult Put()
        {
            return Ok("Hallo PUT!");
        }

        [HttpPost]
        public IActionResult Post()
        {
            return Ok("Hallo POST!");
        }

        [HttpDelete]
        public IActionResult Delete()
        {
            return Ok("Hallo DELETE!");
        }
    }
}

