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
        #region IData extensions

        public static CreateCategoryFunc Object(this IData category, object obj, string name)
        {
            return category.From(nameof(Object), name, obj);
        }

        public static CreateCategoryFunc Field(this IData category, object obj, string name)
        {
            return category.From(nameof(Field), name, obj);
        }

        public static CreateCategoryFunc Property(this IData category, object obj, string name)
        {
            return category.From(nameof(Property), name, obj);
        }

        public static CreateCategoryFunc Argument(this IData category, object obj, string name)
        {
            return category.From(nameof(Argument), name, obj);
        }

        public static CreateCategoryFunc Variable(this IData category, object obj, string name)
        {
            return category.From(nameof(Variable), name, obj);
        }

        #endregion

        #region IAction extensions

        public static CreateCategoryFunc Started(this IAction category, string name)
        {
            return category.From(nameof(Action), name, nameof(Started));
        }

        public static CreateCategoryFunc Cancelled(this IAction category, string name, string reason = null)
        {
            return log =>
            {
                log.Message(reason);
                return (nameof(Action), name, nameof(Cancelled));
            };
        }

        public static CreateCategoryFunc Failed(this IAction category, string name, Exception exception = null)
        {
            return log =>
            {
                log.Exception(exception);
                return (nameof(Action), name, nameof(Failed));
            };
        }

        public static CreateCategoryFunc Finished(this IAction category, string name)
        {
            return category.From(nameof(Action), name, nameof(Finished));
        }

        #endregion

        /// <summary>
        /// Allows to create any type of snapshot.
        /// </summary>
        /// <returns></returns>
        private static CreateCategoryFunc From(this ILogCategory category, string categoryName, string objectName, object obj)
        {
            return log => (categoryName, objectName, obj);
        }
    }
}