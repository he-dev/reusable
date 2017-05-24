using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Colin.Data;
using Reusable.Colin.Services;

namespace Reusable.Colin.Collections
{
    public static class ArgumentLookupExtensions
    {
        [NotNull]
        public static IEnumerable<string> AnonymousValues([NotNull] this ILookup<ImmutableNameSet, string> arguments) => arguments[ImmutableNameSet.Empty];

        [CanBeNull]
        public static ImmutableNameSet CommandName([NotNull] this ILookup<ImmutableNameSet, string> arguments)
        {
            // Command-name is the first anonymous value (0).
            var commandName = arguments.AnonymousValues().FirstOrDefault();
            return string.IsNullOrEmpty(commandName) ? null : ImmutableNameSet.Create(commandName);
        }

        [CanBeNull]
        public static Services.CommandExecutor Executor([NotNull][ItemNotNull] this ILookup<ImmutableNameSet, string> arguments, [NotNull] CommandCollection commandCollection)
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
                commandCollection.TryGetValue(commandName, out Services.CommandExecutor command)
                    ? command
                    : default(Services.CommandExecutor);
        }

        internal static bool Contains(this ILookup<ImmutableNameSet, string> arguments, CommandParameter commandParameter)
        {
            return
                commandParameter.Position > 0
                    ? arguments.AnonymousValues().ElementAtOrDefault(commandParameter.Position) != null
                    : arguments.Contains(commandParameter.Name);
        }

        [NotNull]
        internal static IEnumerable<string> Parameter(this ILookup<ImmutableNameSet, string> arguments, CommandParameter commandParameter)
        {
            return
                commandParameter.Position > 0
                    ? new[] { arguments.AnonymousValues().ElementAtOrDefault(commandParameter.Position) }
                    : arguments[commandParameter.Name];
        }
    }
}