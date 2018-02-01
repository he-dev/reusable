using System;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.SemanticExtensions
{
    /// <summary>
    /// This class provides helper methods for creating various types of snapshots.
    /// </summary>
    [PublicAPI]
    public static class LogCategoryExtensions
    {
        public static CreateCategoryFunc Object(this ILogCategory category, object obj, string name)
        {
            return category.From(nameof(Object), name, obj);
        }

        public static CreateCategoryFunc Field(this ILogCategory category, object obj, string name)
        {
            return category.From(nameof(Field), name, obj);
        }

        public static CreateCategoryFunc Property(this ILogCategory category, object obj, string name)
        {
            return category.From(nameof(Property), name, obj);
        }

        public static CreateCategoryFunc Action(this ILogCategory category, object obj, string name)
        {
            return category.From(nameof(Action), name, obj);
        }

        public static CreateCategoryFunc Argument(this ILogCategory category, object obj, string name)
        {
            return category.From(nameof(Argument), name, obj);
        }

        public static CreateCategoryFunc Variable(this ILogCategory category, object obj, string name)
        {
            return category.From(nameof(Variable), name, obj);
        }

        /// <summary>
        /// Allows to create any type of snapshot.
        /// </summary>
        /// <returns></returns>
        private static CreateCategoryFunc From(this ILogCategory category, string categoryName, string objectName, object obj)
        {
            return () => (categoryName, objectName, obj);
        }
    }
}