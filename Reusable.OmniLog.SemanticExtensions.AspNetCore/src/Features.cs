using Reusable.Beaver;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore
{
    public static class Features
    {
        public static readonly Feature LogResponseBody = new Feature(nameof(LogResponseBody)) { Tags = { "Telemetry" } };
    }
}