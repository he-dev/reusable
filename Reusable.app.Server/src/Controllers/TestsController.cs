using Microsoft.AspNetCore.Mvc;
using Reusable.OmniLog;
using Reusable.OmniLog.Utilities.AspNetCore.Mvc.Filters;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Extensions;


namespace Reusable.Apps.Server.Controllers
{
    //[ApiVersion("1.0")]
    [Route("/api/[controller]")]
    [ServiceFilter(typeof(LogResponseBody))]
    public class TestsController : Controller
    {
        private readonly ILogger _logger;

        public TestsController(ILogger<TestsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(string message)
        {
            _logger.Log(Telemetry.Collect.Application().Metadata("Test", 123));
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