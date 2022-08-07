using System;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Utilities.AspNetCore.Extensions;
using Reusable.Wiretap.Utilities.AspNetCore.Helpers;

namespace Reusable.Wiretap.Utilities.AspNetCore
{
    [PublicAPI]
    public class WiretapMiddlewareConfiguration
    {
        public Func<HttpContext, object> GetCorrelationId { get; set; } = context => context.GetCorrelationHeaderOrDefault();

        public Func<HttpContext, object> GetCorrelationHandle { get; set; } = _ => "HttpRequest";

        public Func<HttpContext, bool> CanLogRequestBody { get; set; } = _ => true;

        public Func<HttpContext, bool> CanLogResponseBody { get; set; } = _ => true;
        
        public Func<HttpContext, bool> CanUpdateOriginalResponseBody { get; set; } = c => !c.Response.StatusCode.In(204, 304);
        
        public Func<HttpRequest, object> TakeRequestSnapshot { get; set; } = LogHelper.TakeRequestSnapshot;

        public Func<HttpResponse, object> TakeResponseSnapshot { get; set; } = LogHelper.TakeResponseSnapshot;

        public Func<HttpContext, Task<string?>> SerializeRequestBody { get; set; } = LogHelper.DumpRequestBody;
    }
}