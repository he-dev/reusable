using System;
using System.Reflection;

namespace Reusable.Extensions
{
    public static class StringPrettifierExtensions
    {
        private static readonly IStringPrettifier Prettifier = new StringPrettifier();
        
        public static string ToPrettyString(this Type type)
        {
            return Prettifier.Render(type);
        }
        
        public static string ToPrettyString(this MethodInfo methodInfo)
        {
            return Prettifier.Render(methodInfo);
        }
    }
}