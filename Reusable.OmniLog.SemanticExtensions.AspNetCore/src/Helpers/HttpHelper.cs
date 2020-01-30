using System;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore.Helpers {
    public static class HttpHelper
    {
        
        public static Func<int, Reusable.Data.Option<LogLevel>> MapStatusCode { get; set; } = statusCode =>
        {
            return statusCode switch
            {
                var x when x >= 500 => LogLevel.Fatal,
                var x when x >= 400 => LogLevel.Error,
                var x when x >= 300 => LogLevel.Warning,
                _ => LogLevel.Information,
            };
        };
    }
}