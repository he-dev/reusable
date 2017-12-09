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
    public static class Snapshot
    {
        public static Func<Log, (string Name, object Object)> Fields(object obj)
        {
            return From(obj, nameof(Fields));
        }

        public static Func<Log, (string Name, object Object)> Properties(object obj)
        {
            return From(obj, nameof(Properties));
        }

        public static Func<Log, (string Name, object Object)> Objects(object obj, string name)
        {
            return From(obj, nameof(Objects), name);
        }        

        public static Func<Log, (string Name, object Object)> Variables(object obj)
        {
            return From(obj, nameof(Variables));
        }

        public static Func<Log, (string Name, object Object)> Arguments(object obj)
        {
            return From(obj, nameof(Arguments));
        }        

        /// <summary>
        /// Allows to create any type of snapshot.
        /// </summary>
        /// <param name="obj">Object that represents a snapshot.</param>
        /// <param name="names">Names of the snapshot. Multiple names are separated by '/'.</param>
        /// <returns></returns>
        private static Func<Log, (string Name, object Object)> From(object obj, params string[] names)
        {
            return log => (names.Where(Conditional.IsNotNullOrEmpty).Join("/"), obj);
        }
    }
}