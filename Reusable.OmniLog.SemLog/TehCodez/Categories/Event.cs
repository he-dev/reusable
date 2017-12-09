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
    public static class Event
    {
        public static Func<Log, (string Name, object Object)> Started(string name)
        {
            return From(name, nameof(Started));
        }

        public static Func<Log, (string Name, object Object)> Finished(string name)
        {
            return From(name, nameof(Finished));
        }

        public static Func<Log, (string Name, object Object)> Failed(string name, Exception exception = null)
        {
            return log =>
            {
                if (!(exception is null))
                {
                    log.Exception(exception);
                }
                return From(name, nameof(Failed))(log);
            };
        }

        private static Func<Log, (string Name, object Object)> From(object obj, params string[] names)
        {
            return log => (names.Prepend(Category.Events.ToString()).Where(Conditional.IsNotNullOrEmpty).Join("/"), obj);
        }
    }
}