using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Reusable.OmniLog
{
    public static class Reflection
    {
        internal static IEnumerable<(SoftString Key, object Value)> GetProperties(object obj)
        {
            if (obj is null)
            {
                yield break;
            }

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                yield return (Key: (SoftString) property.Name, Value: property.GetValue(obj));
            }
        }

        public static string CallerMemberName([CallerMemberName] string callerMemberName = null)
        {
            return callerMemberName;
        }

        public static (string CallerMemberName, int CallerLineNumber, string CallerFilePath) CallerInfo(
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null
        )
        {
            return (callerMemberName, callerLineNumber, callerFilePath);
        }
    }
}