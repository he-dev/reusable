using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable
{
    public interface IPrettyString
    {
        [NotNull, ContractAnnotation("type: null => halt")]
        string Render([NotNull] Type type, bool includeNamespace = false);
        
        [NotNull, ContractAnnotation("methodInfo: null => halt")]
        string Render([NotNull] MethodInfo methodInfo, bool includeNamespace = false);
    }
    
    public class PrettyString : IPrettyString
    {
        //public static string ToPrettyString<TException>(this TException exception, ExceptionOrder order = ExceptionOrder.Ascending, int indentWidth = 4) where TException : Exception
        //{
        //    var nodes = exception.SelectMany();
        //    var exceptionStrings = nodes.Select(n => BuildExceptionString(n.Exception, n.Depth)).ToList();

        //    if (order == ExceptionOrder.Ascending) { exceptionStrings.Reverse(); }

        //    return string.Join(Environment.NewLine, exceptionStrings);

        //    string BuildExceptionString(Exception ex, int depth)
        //    {
        //        return
        //            new ExceptionStringBuilder(indentWidth)
        //                .AppendExceptionMessage(ex, depth)
        //                .AppendInnerExceptionCount(ex, depth)
        //                .AppendExceptionProperties(ex, depth)
        //                .AppendExceptionData(ex, depth)
        //                .AppendExceptionStackTrace(ex, depth);
        //    }
        //}

        public string Render(Type type, bool includeNamespace)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            
            using (var codeDomProvider = CodeDomProvider.CreateProvider("C#"))
            using (var stringWriter = new StringWriter())
            {
                var typeReferenceExpression = new CodeTypeReferenceExpression(type);
                codeDomProvider.GenerateCodeFromExpression(typeReferenceExpression, stringWriter, new CodeGeneratorOptions());
                var typeName = stringWriter.GetStringBuilder().ToString();
                return includeNamespace ? typeName : RemoveNamespace(typeName);
            }
        }

        public string Render(MethodInfo method, bool includeNamespace)
        {
            if (method == null) { throw new ArgumentNullException(nameof(method)); }

            var parameters = method.GetParameters().Select(p => $"{Render(p.ParameterType, includeNamespace)} {p.Name}");

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
                .Append(" ").Append(Render(method.ReturnType, includeNamespace))
                .Append(" ").Append(method.Name)
                .Append("(").Append(string.Join(", ", parameters)).Append(") { ... }")
                .Append(" } ")
                .ToString();

            return includeNamespace ? signature : RemoveNamespace(signature);
        }

        private static string RemoveNamespace(string typeName)
        {
            return Regex.Replace(typeName, @"[a-z0-9_]+\.", string.Empty, RegexOptions.IgnoreCase);
        }
    }    

    //public enum ExceptionOrder
    //{
    //    Ascending,
    //    Descending
    //}
}
