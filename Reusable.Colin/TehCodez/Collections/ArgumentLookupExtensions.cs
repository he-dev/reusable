using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Commands;
using Reusable.Colin.Data;
using Reusable.Colin.Services;

namespace Reusable.Colin.Collections
{
    public static class ArgumentLookupExtensions
    {
        [NotNull]
        public static IEnumerable<string> AnonymousArguments([NotNull] this ArgumentLookup @this) => @this[ImmutableNameSet.Empty];

        [CanBeNull]
        public static ImmutableNameSet CommandName([NotNull] this ArgumentLookup @this)
        {
            // Command-name is at argument-0.
            var commandName = @this.AnonymousArguments().FirstOrDefault();
            return string.IsNullOrEmpty(commandName) ? null : ImmutableNameSet.Create(commandName);
        }

        [NotNull]
        public static IEnumerable<(CommandInvoker CommandInvoker, ArgumentLookup Arguments)> FindCommands([NotNull][ItemNotNull] this IEnumerable<ArgumentLookup> arguments, [NotNull] CommandLine commandLine)
        {
            return
                from a in arguments
                let c = GetCommandOrDefault(a.CommandName())
                where c != null
                select (c, a);

            CommandInvoker GetCommandOrDefault(ImmutableNameSet name)
            {
                return
                    commandLine.TryGetValue(name, out CommandInvoker command) ||
                    commandLine.TryGetValue(CommandLine.DefaultCommandName, out command)
                        ? command
                        : default(CommandInvoker);
            }
        }

        internal static bool Contains(this ArgumentLookup @this, CommandParameter commandParameter)
        {
            return
                commandParameter.Position > 0
                    ? @this.AnonymousArguments().ElementAtOrDefault(commandParameter.Position) != null
                    : @this.Contains(commandParameter.Name);
        }

        [NotNull]
        internal static IEnumerable<string> Parameter(this ArgumentLookup @this, CommandParameter commandParameter)
        {
            return
                commandParameter.Position > 0 
                    ? new[] { @this.AnonymousArguments().ElementAtOrDefault(commandParameter.Position) } 
                    : @this[commandParameter.Name];
        }
    }
}