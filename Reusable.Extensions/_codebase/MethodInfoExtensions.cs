using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Reusable.Extensions
{
    public static class MethodInfoExtensions
    {
        public static string ToShortString(this MethodInfo methodInfo)
        {
            if (methodInfo == null) { throw new ArgumentNullException(nameof(methodInfo)); }

            var signature =
                new StringBuilder()
                .Append(methodInfo.ReturnType.FullName)
                .Append(" ").Append(methodInfo.DeclaringType?.FullName)
                .Append(".").Append(methodInfo.Name)
                .Append("(")
                .Append(string.Join(", ", methodInfo.GetParameters().Select(p => $"{p.ParameterType.ToShortString()} {p.Name}")))
                .Append(")")
                .ToString();

            return signature;
        }
    }
}
