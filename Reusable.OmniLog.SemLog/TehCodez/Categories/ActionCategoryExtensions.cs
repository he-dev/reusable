using System;
using System.Linq;
using System.Linq.Custom;
using Reusable.Extensions;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.SemanticExtensions
{
    /// <summary>
    /// This class provides helper methods for creating various types of events.
    /// </summary>
    public static class ActionCategoryExtensions
    {
        public static CreateCategoryFunc Started(this IActionCategory category, string name)
        {
            return category.Actions(name, nameof(Started));
        }

        public static CreateCategoryFunc Cancelled(this IActionCategory category, string name)
        {
            return log =>
            {
                log.Level(LogLevel.Warning);
                return category.Actions(name, nameof(Cancelled))(log);
            };
        }

        public static CreateCategoryFunc Finished(this IActionCategory category, string name)
        {
            return category.Actions(name, nameof(Finished));
        }

        public static CreateCategoryFunc Failed(this IActionCategory category, string name, Exception exception = null)
        {
            return log =>
            {
                if (!(exception is null))
                {
                    log.Exception(exception);
                }
                log.Level(LogLevel.Error);
                return category.Actions(name, nameof(Failed))(log);
            };
        }

        private static CreateCategoryFunc Actions(this IActionCategory category, object obj, params string[] names)
        {
            return log => (names.Prepend(nameof(Actions)).Where(Conditional.IsNotNullOrEmpty).Join("/"), obj);
        }
    }
}