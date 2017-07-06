using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.CommandLine.Data;
using Reusable.CommandLine.Services;

namespace Reusable.CommandLine.Collections
{
    public static class ArgumentLookupExtensions
    {
        [NotNull]
        public static IEnumerable<string> Anonymous([NotNull] this ILookup<IImmutableNameSet, string> arguments) => arguments[ImmutableNameSet.Empty];

        [CanBeNull]
        public static IImmutableNameSet CommandName([NotNull] this ILookup<IImmutableNameSet, string> arguments)
        {
            // Command-name is the first anonymous value (0).
            var commandName = arguments.Anonymous().FirstOrDefault();
            return string.IsNullOrEmpty(commandName) ? null : ImmutableNameSet.Create(commandName);
        }

        internal static bool Contains(this ILookup<IImmutableNameSet, string> arguments, ArgumentMetadata argument)
        {
            return
                argument.Position > 0
                    ? arguments.Anonymous().ElementAtOrDefault(argument.Position) != null
                    : arguments.Contains(argument.Name);
        }

        [NotNull]
        internal static IEnumerable<string> Parameter(this ILookup<IImmutableNameSet, string> arguments, ArgumentMetadata argument)
        {
            if (argument.Position > 0)
            {
                yield return arguments.Anonymous().ElementAtOrDefault(argument.Position);
            }
            else
            {
                foreach (var value in arguments[argument.Name])
                {
                    yield return value;
                }
            }
        }

        public static string ToCommandLineString(this ILookup<IImmutableNameSet, string> arguments, string format)
        {
            return string.Join(" ", arguments.Select(argument => argument.ToCommandLineString(format)));
        }
    }
}