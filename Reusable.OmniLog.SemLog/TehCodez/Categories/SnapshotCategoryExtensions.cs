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
    public static class SnapshotCategoryExtensions
    {
        public static CreateCategoryFunc Fields(this ISnapshotCategory category, object obj, string name = null)
        {
            return category.From(obj, nameof(Fields), name);
        }

        public static CreateCategoryFunc Properties(this ISnapshotCategory category, object obj, string name = null)
        {
            return category.From(obj, nameof(Properties), name);
        }

        public static CreateCategoryFunc Objects(this ISnapshotCategory category, object obj, string name)
        {
            return category.From(obj, nameof(Objects), name);
        }

        public static CreateCategoryFunc Variables(this ISnapshotCategory category, object obj, string name = null)
        {
            return category.From(obj, nameof(Variables), name);
        }

        public static CreateCategoryFunc Arguments(this ISnapshotCategory category, object obj, string name = null)
        {
            return category.From(obj, nameof(Arguments), name);
        }

        public static CreateCategoryFunc Results(this ISnapshotCategory category, object obj, string name = null)
        {
            return category.From(obj, nameof(Results), name);
        }

        /// <summary>
        /// Allows to create any type of snapshot.
        /// </summary>
        /// <returns></returns>
        private static CreateCategoryFunc From(this ISnapshotCategory category, object obj, params string[] names)
        {
            return log => (names.Where(Conditional.IsNotNullOrEmpty).Join("/"), obj);
        }
    }
}