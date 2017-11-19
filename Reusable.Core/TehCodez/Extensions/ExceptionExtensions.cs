using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.Extensions
{
    public static class ExceptionExtensions
    {
        [NotNull]
        public static IEnumerable<(Exception Exception, int Depth)> SelectMany([NotNull] this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var exceptionStack = new Stack<(Exception Exception, int Depth)>();
            exceptionStack.Push((exception, 0));

            while (exceptionStack.Any())
            {
                var current = exceptionStack.Pop();
                yield return current;

                switch (current.Exception)
                {
                    case AggregateException ex:
                        foreach (var innerException in ex.InnerExceptions)
                        {
                            exceptionStack.Push((innerException, current.Depth + 1));
                        }
                        break;

                    case Exception ex when ex.InnerException != null:
                        exceptionStack.Push((ex.InnerException, current.Depth + 1));
                        break;
                }
            }
        }

        public static IEnumerable<(string Name, object Value)> GetPropertiesExcept<TExcept>(this Exception exception) where TExcept : Exception
        {
            var propertyFlags = BindingFlags.Instance | BindingFlags.Public;

            return
                exception
                    .GetType()
                    .GetProperties(propertyFlags)
                    .Except(typeof(TExcept).GetProperties(propertyFlags), x => x.Name)
                    .Select(p => (Name: p.Name, Value: p.GetValue(exception)))
                    .Where(p => p.Value is string);
        }

        public static IEnumerable<(object Key, object Value)> GetData(this Exception exception)
        {
            return from object key in exception.Data.Keys select (Key: key, Value: exception.Data[key]);
        }

        public static IEnumerable<(MethodInfo Caller, string FileName, int LineNumber)> GetStackTrace(this Exception exception)
        {
            var stackTrace = new StackTrace(exception, true);
            var stackFrames = stackTrace.GetFrames() ?? Enumerable.Empty<StackFrame>();
            foreach (var stackFrame in stackFrames)
            {
                yield return
                (
                    Caller: stackFrame.GetMethod() as MethodInfo,
                    FileName: stackFrame.GetFileName().IIf(Conditional.IsNullOrEmpty, Path.GetFileName),
                    LineNumber: stackFrame.GetFileLineNumber()
                );
            }
        }
    }
}