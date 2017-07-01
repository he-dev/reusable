using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Colin.Data;
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
        public static CommandMapping Map([NotNull][ItemNotNull] this ILookup<IImmutableNameSet, string> arguments, [NotNull] CommandCollection commandCollection)
        {
            if (commandCollection.Count == 1)
            {
                return commandCollection.Single().Value;
            }

            // Default command is used whenever there is no command name or there are no arguments.
            var commandName = 
                arguments.Any() 
                    ? arguments.CommandName() ?? ImmutableNameSet.DefaultCommandName 
                    : ImmutableNameSet.DefaultCommandName;

            return
                commandCollection.TryGetValue(commandName, out CommandMapping command)
                    ? command
                    : default(CommandMapping);
        }

        internal static bool Contains(this ILookup<IImmutableNameSet, string> arguments, CommandParameter commandParameter)
        {
            return
                commandParameter.Position > 0
                    ? arguments.AnonymousValues().ElementAtOrDefault(commandParameter.Position) != null
                    : arguments.Contains(commandParameter.Name);
        }

        [NotNull]
        internal static IEnumerable<string> Parameter(this ILookup<IImmutableNameSet, string> arguments, CommandParameter commandParameter)
        {
            return
                commandParameter.Position > 0
                    ? new[] { arguments.AnonymousValues().ElementAtOrDefault(commandParameter.Position) }
                    : arguments[commandParameter.Name];
        }

        public static string ToCommandLine(this ILookup<IImmutableNameSet, string> arguments, string format)
        {
            return string.Join(" ", arguments.Select(argument => argument.ToCommandLine(format)));
        }
    }
}