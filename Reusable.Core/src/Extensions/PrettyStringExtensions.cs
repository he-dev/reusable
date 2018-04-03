using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Reusable.Extensions
{
    public static class PrettyStringExtensions
    {
        private static readonly IPrettyString Prettifier = new PrettyString();
        
        [NotNull, ContractAnnotation("type: null => halt")]
        public static string ToPrettyString([NotNull] this Type type, bool includeNamespace = false)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return Prettifier.Render(type, includeNamespace);
        }
        
        [NotNull, ContractAnnotation("methodInfo: null => halt")]
        public static string ToPrettyString([NotNull] this MethodInfo methodInfo, bool includeNamespace = false)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

            return Prettifier.Render(methodInfo, includeNamespace);
        }
    }
}