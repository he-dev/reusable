using System;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.OmniLog.Utilities.AspNetCore.Helpers;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Utilities.AspNetCore.Extensions;

namespace Reusable.OmniLog.Utilities.AspNetCore
{
    [PublicAPI]
    public class SemanticLoggerConfig
    {
        public Func<HttpContext, object> GetCorrelationId { get; set; } = context => context.GetCorrelationHeaderOrDefault();

        public Func<HttpContext, object> GetCorrelationHandle { get; set; } = _ => "HttpRequest";

        public Func<HttpContext, bool> CanLogRequestBody { get; set; } = _ => true;

        public Func<HttpContext, bool> CanLogResponseBody { get; set; } = _ => true;
        
        public Func<HttpContext, bool> CanUpdateOriginalResponseBody { get; set; } = c => !c.Response.StatusCode.In(204, 304);
        
        public Func<HttpContext, object> TakeRequestSnapshot { get; set; } = LogHelper.TakeRequestSnapshot;

        public Func<HttpContext, object> TakeResponseSnapshot { get; set; } = LogHelper.TakeResponseSnapshot;

        public Func<int, LogLevel> MapStatusCode { get; set; } = LogHelper.MapStatusCode;

        public Func<HttpContext, Task<string?>> SerializeRequestBody { get; set; } = LogHelper.SerializeRequestBody;
    }
}