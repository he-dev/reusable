using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions.AspNetCore.Extensions;
using Reusable.OmniLog.SemanticExtensions.AspNetCore.Helpers;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore
{
    [PublicAPI]
    public class SemanticLoggerConfig
    {
        public Func<HttpContext, object> GetCorrelationId { get; set; } = context => context.GetCorrelationId();

        public Func<HttpContext, object> GetCorrelationHandle { get; set; } = _ => "HttpRequest";

        public Func<HttpContext, bool> CanLogRequestBody { get; set; } = _ => true;

        public Func<HttpContext, bool> CanLogResponseBody { get; set; } = _ => true;

        /// <summary>
        /// There are some http-status codes where the response must not be written to.
        /// </summary>
        public HashSet<int> IgnoreResponseHttpStatusCodes { get; set; } = new HashSet<int> { 204, 304 };

        public Action<ILogger, HttpContext, string?> LogRequest { get; set; } = LogHelper.LogRequest;

        public Action<ILogger, HttpContext, string?> LogResponse { get; set; } = LogHelper.LogResponse;

        public Action<ILogger, HttpContext, Exception> LogError { get; set; } = LogHelper.LogError;

        public Func<HttpContext, Task<string?>> SerializeRequestBody { get; set; } = LogHelper.SerializeRequestBody;
    }
}