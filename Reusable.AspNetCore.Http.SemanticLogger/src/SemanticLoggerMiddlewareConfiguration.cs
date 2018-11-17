using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Reusable.OmniLog;

namespace Reusable.AspNetCore.Http
{
    public class SemanticLoggerMiddlewareConfiguration
    {
        public Action<ILogScope, HttpContext> ConfigureScope { get; set; }
    }
}