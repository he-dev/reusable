using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Reusable.Collections;

namespace Reusable
{
    public static class ExceptionPrettifier
    {
        public static string ToPrettyString<TException>(this TException exception, ExceptionOrder order = ExceptionOrder.Ascending, int indentWidth = 4) where TException : Exception
        {
            var exceptionStrings = new List<StringBuilder>();

            var nodes = exception.GetInnerExceptions();

            var indent = new Func<int, int, string>((depth, nestedDepth) => new string(' ', indentWidth * (depth + nestedDepth)));

            foreach (var node in nodes)
            {
                var text = new StringBuilder();

                var depth = (int)node.Depth;

                if (text.Length > 0) { text.AppendLine(); }

                //text.Append(indent(0, level)).AppendLine($"{new string('.', level + 1)}");
                text.Append(indent(0, depth)).AppendLine($"{node.GetType().Name}: \"{node.Value.Message}\"");

                if (node.Value is AggregateException)
                {
                    text.Append(indent(1, depth)).AppendLine($"InnerExceptions: \"{((AggregateException)node).InnerExceptions.Count}\"");
                }

                foreach (var property in node.Value.GetPropertiesExcept<Exception>())
                {
                    text.Append(indent(1, depth)).AppendLine($"{property.Name}: \"{property.Value}\"");
                }

                foreach (var property in node.Value.GetData())
                {
                    text.Append(indent(1, depth)).AppendLine($"Data[{property.Key}]: \"{property.Value}\"");
                }

                text.Append(indent(1, depth)).AppendLine($"StackTrace:");

                foreach (var stackTrace in node.Value.GetStackTrace() ?? System.Linq.Enumerable.Empty<dynamic>())
                {
                    text.Append(indent(2, depth)).AppendLine($"{stackTrace.Caller} in \"{stackTrace.FileName}\" Ln {stackTrace.LineNumber}");
                }

                exceptionStrings.Add(text);
            }

            if (order == ExceptionOrder.Ascending) { exceptionStrings.Reverse(); }

            return string.Join(Environment.NewLine, exceptionStrings);
        }

        private static IEnumerable<dynamic> GetPropertiesExcept<TExceptException>(this Exception exception) where TExceptException : Exception
        {
            var propertyFlags = BindingFlags.Instance | BindingFlags.Public;

            var properties = exception.GetType()
                .GetProperties(propertyFlags)
                .Except(typeof(TExceptException).GetProperties(propertyFlags), x => x.Name)
                .Select(p => new { p.Name, Value = p.GetValue(exception) })
                .Where(p => p.Value != null && !string.IsNullOrEmpty(p.Value as string));
            return properties;
        }

        private static IEnumerable<dynamic> GetData(this Exception exception)
        {
            foreach (var key in exception.Data.Keys)
            {
                yield return new { Key = key, Value = exception.Data[key] };
            }
        }

        private static IEnumerable<dynamic> GetStackTrace(this Exception exception)
        {
            var stackTrace = new StackTrace(exception, true);
            var stackFrames = stackTrace.GetFrames();
            var result = stackFrames?.Select(sf => new
            {
                Caller = (sf.GetMethod() as MethodInfo)?.ToShortString() ?? string.Empty,
                FileName = Path.GetFileName(sf.GetFileName()),
                LineNumber = sf.GetFileLineNumber(),
            });
            return result;
        }
    }

    public enum ExceptionOrder
    {
        Ascending,
        Descending
    }
}
