using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Commands;
using Reusable.Colin.Data;

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
        public static IEnumerable<(ICommand Command, ArgumentLookup Arguments)> FindCommands([NotNull][ItemNotNull] this IEnumerable<ArgumentLookup> arguments, [NotNull] CommandLine commandLine)
        {
            return
                from a in arguments
                let c = GetCommandOrDefault(a.CommandName())
                where c != null
                select (c, a);

            ICommand GetCommandOrDefault(ImmutableNameSet name)
            {
                return
                    commandLine.TryGetValue(name, out ICommand command) ||
                    commandLine.TryGetValue(DefaultCommand.Name, out command)
                        ? command
                        : default(ICommand);
            }
        }

        internal static bool Contains(this ArgumentLookup @this, ParameterInfo parameter)
        {
            return
                parameter.Position > 0
                    ? @this.AnonymousArguments().ElementAtOrDefault(parameter.Position) != null
                    : @this.Contains(parameter.Names);
        }

        [NotNull]
        internal static IEnumerable<string> Parameter(this ArgumentLookup @this, ParameterInfo parameter)
        {
            return
                parameter.Position > 0 
                    ? new[] { @this.AnonymousArguments().ElementAtOrDefault(parameter.Position) } 
                    : @this[parameter.Names];
        }
    }
}