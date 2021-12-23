using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Extensions
{
    public static class LoggerNodeExtensions
    {
        public static T Enable<T>(this T node) where T : ILoggerNode
        {
            return node.Also(x => x.Enabled = true);
        }
    }
}