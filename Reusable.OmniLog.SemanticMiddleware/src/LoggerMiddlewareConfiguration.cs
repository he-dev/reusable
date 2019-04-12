using System;
using Microsoft.AspNetCore.Http;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class LoggerMiddlewareConfiguration
    {
        public Action<ILogScope, HttpContext> ConfigureScope { get; set; }
    }
}