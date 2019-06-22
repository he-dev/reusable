using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Reusable.Flawless.Helpers
{
    internal static class TypeExtensions
    {
        public static bool IsClosure(this Type type)
        {
            return
                type.Name.StartsWith("<>c__DisplayClass") &&
                type.IsDefined(typeof(CompilerGeneratedAttribute));
        }
    }
}