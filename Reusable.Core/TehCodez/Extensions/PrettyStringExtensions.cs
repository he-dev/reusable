using System;
using System.Reflection;

namespace Reusable.Extensions
{
    public static class PrettyStringExtensions
    {
        private static readonly IPrettyString Prettifier = new PrettyString();
        
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