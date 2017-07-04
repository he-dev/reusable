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
        public static IEnumerable<string> AnonymousValues([NotNull] this ILookup<IImmutableNameSet, string> arguments) => arguments[ImmutableNameSet.Empty];

        [CanBeNull]
        public static IImmutableNameSet CommandName([NotNull] this ILookup<IImmutableNameSet, string> arguments)
        {
            // Command-name is the first anonymous value (0).
            var commandName = arguments.AnonymousValues().FirstOrDefault();
            return string.IsNullOrEmpty(commandName) ? null : ImmutableNameSet.Create(commandName);
        }

        [CanBeNull]
        public static CommandMetadata FindCommand([NotNull][ItemNotNull] this ILookup<IImmutableNameSet, string> arguments, [NotNull] CommandContainer commands)
        {
            // The help-command requires special treatment and does not count as a "real" command so exclude it from count.
            bool IsHelpCommand(IImmutableNameSet name) => name.Overlaps(new[] { "help" });

            var commandCount = commands.Count(c => !IsHelpCommand(c.Value.CommandName));

            if (commandCount == 1)
            {
                return commands.Single(c => !IsHelpCommand(c.Value.CommandName)).Value;
            }

            // Default command is used whenever there is no command name or there are no arguments.
            //var commandName =
            //    arguments.Any()
            //        ? arguments.CommandName() ?? ImmutableNameSet.DefaultCommandName
            //        : ImmutableNameSet.DefaultCommandName;

            return commands.TryGetValue(arguments.CommandName(), out var command) ? command : default(CommandMetadata);
        }

        internal static bool Contains(this ILookup<IImmutableNameSet, string> arguments, ArgumentMetadata argument)
        {
            return
                argument.Position > 0
                    ? arguments.AnonymousValues().ElementAtOrDefault(argument.Position) != null
                    : arguments.Contains(argument.Name);
        }

        [NotNull]
        internal static IEnumerable<string> Parameter(this ILookup<IImmutableNameSet, string> arguments, ArgumentMetadata argument)
        {
            return
                argument.Position > 0
                    ? new[] { arguments.AnonymousValues().ElementAtOrDefault(argument.Position) }
                    : arguments[argument.Name];
        }

        public static string ToCommandLineString(this ILookup<IImmutableNameSet, string> arguments, string format)
        {
            return string.Join(" ", arguments.Select(argument => argument.ToCommandLineString(format)));
        }
    }
}