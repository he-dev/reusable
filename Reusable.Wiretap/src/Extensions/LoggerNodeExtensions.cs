using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Extensions
{
    public static class LoggerNodeExtensions
    {
        // public static T Enable<T>(this T node) where T : ILoggerNode
        // {
        //     return node.Also(x => x.Enabled = true);
        // }
        //
        // public static T Disable<T>(this T node) where T : ILoggerNode
        // {
        //     return node.Also(x => x.Enabled = false);
        // }
        
        /// <summary>
        /// Gets logger-node of the specified type.
        /// </summary>
        public static T Node<T>(this IEnumerable<ILoggerNode> logger) where T : ILoggerNode
        {
            return logger.OfType<T>().SingleOrThrow($"There must be exactly one node of type '{typeof(T).ToPrettyString()}'.");
        }
    }
}