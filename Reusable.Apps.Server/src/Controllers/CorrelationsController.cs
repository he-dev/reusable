using System;
using Microsoft.AspNetCore.Mvc;

namespace Reusable.Apps.Server.Controllers
{
    //[ApiVersion("1.0")]
    [Route("/api/[controller]")]
    public class CorrelationsController : Controller
    {
        [HttpGet]
        public IActionResult NewCorrelation()
        {
            return Ok(new
            {
                Id = $"{Guid.NewGuid():N}".ToUpper()
            });
        }
    }
}

