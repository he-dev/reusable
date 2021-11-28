using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Extensions
{
    public static class LoggerNodeExtensions
    {
        public static T Enable<T>(this T node) where T : ILoggerNode
        {
            return node.Also(x => x.Enabled = true);
        }
    }
}