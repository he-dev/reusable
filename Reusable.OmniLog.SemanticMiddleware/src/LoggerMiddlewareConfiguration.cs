using System;
using Microsoft.AspNetCore.Http;

namespace Reusable.OmniLog
{
    public class LoggerMiddlewareConfiguration
    {
        public Action<ILogScope, HttpContext> ConfigureScope { get; set; }
    }
}