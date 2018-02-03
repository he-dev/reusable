using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Reusable.Exceptionize;

namespace Reusable.OmniLog
{
    public static class Reflection
    {
        public static IEnumerable<(SoftString PropertyName, object PropertyValue)> GetProperties(object obj)
        {
            if (obj is null)
            {
                yield break;
            }

            if (!obj.GetType().Name.StartsWith("<>f__AnonymousType1"))
            {
                DynamicException.Factory.CreateDynamicException(
                    $"ObjectType{nameof(Exception)}",
                    "Object must be an anonymous type. Try new { ... }",
                    null
                );
            }

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                yield return (property.Name, property.GetValue(obj));
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