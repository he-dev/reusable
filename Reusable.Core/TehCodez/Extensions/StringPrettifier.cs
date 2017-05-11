using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Reusable.Extensions
{
    public static class StringPrettifier
    {
        public static string ToPrettyString<TException>(this TException exception, ExceptionOrder order = ExceptionOrder.Ascending, int indentWidth = 4) where TException : Exception
        {
            var nodes = exception.Flatten();
            var exceptionStrings = nodes.Select(n => BuildExceptionString(n.Exception, n.Depth)).ToList();

            if (order == ExceptionOrder.Ascending) { exceptionStrings.Reverse(); }

            return string.Join(Environment.NewLine, exceptionStrings);

            string BuildExceptionString(Exception ex, int depth)
            {
                return
                    new ExceptionStringBuilder(indentWidth)
                        .AppendExceptionMessage(ex, depth)
                        .AppendInnerExceptionCount(ex, depth)
                        .AppendExceptionProperties(ex, depth)
                        .AppendExceptionData(ex, depth)
                        .AppendExceptionStackTrace(ex, depth);
            }
        }

        public static string ToPrettyString(this Type type)
        {
            using (var codeDomProvider = CodeDomProvider.CreateProvider("C#"))
            using (var stringWriter = new StringWriter())
            {
                var typeReferenceExpression = new CodeTypeReferenceExpression(type);
                codeDomProvider.GenerateCodeFromExpression(typeReferenceExpression, stringWriter, new CodeGeneratorOptions());
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        public static string ToPrettyString(this MethodInfo method)
        {
            if (method == null) { throw new ArgumentNullException(nameof(method)); }

            var parameters = method.GetParameters().Select(p => $"{p.ParameterType.ToPrettyString()} {p.Name}");

            // public/internal/protected/private [static] [abstract/virtual/override] retVal

            var accessModifier = new(bool AccessModifier, string Name)[]
            {
                (method.IsPublic, "public"),
                (method.IsAssembly, "internal"),
                (method.IsPrivate, "private"),
                (method.IsFamily, "protected"),
            }
            .First(x => x.AccessModifier).Name;

            var inheritanceModifier = new(bool InheritanceModifier, string Name)[]
            {
                (method.IsAbstract, "abstract"),
                (method.IsVirtual, "virtual"),
                (method.GetBaseDefinition() != method, "override"),
            }
            .FirstOrDefault(x => x.InheritanceModifier).Name;

            var signature = new StringBuilder()
                .Append(method.DeclaringType?.FullName)
                .Append(" { ")
                .Append(accessModifier)
                .Append(method.IsStatic ? " static" : string.Empty)
                .Append(string.IsNullOrEmpty(inheritanceModifier) ? string.Empty : $" {inheritanceModifier}")
                .Append(method.GetCustomAttribute<AsyncStateMachineAttribute>() == null ? string.Empty : " async")
                .Append(" ").Append(method.ReturnType.ToPrettyString())
                .Append(" ").Append(method.Name)
                .Append("(").Append(string.Join(", ", parameters)).Append(") { ... }")
                .Append(" } ")
                .ToString();

            return signature;
        }       
    }

    public enum ExceptionOrder
    {
        Ascending,
        Descending
    }
}
