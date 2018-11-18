using Microsoft.AspNetCore.Mvc;
using Reusable.OmniLog;
using Reusable.OmniLog.SemanticExtensions;
using Abstraction = Reusable.OmniLog.SemanticExtensions.Abstraction;


namespace Reusable.Apps.Server.Controllers
{
    //[ApiVersion("1.0")]
    [Route("/api/[controller]")]
    public class TestsController : Controller
    {
        private readonly ILogger<TestsController> _logger;

        public TestsController(ILogger<TestsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(string message)
        {
            _logger.Log(Abstraction.Layer.Infrastructure().Meta(new { TestMeta = 123 }));
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

