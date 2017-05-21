using System;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Commands;
using Reusable.Colin.Data;
using Reusable.Colin.Services;

namespace Reusable.Colin
{
    public static class CommandLineBuilderExtensions
    {
        [NotNull]
        public static CommandLineBuilder Register<TCommand, TParameter>(this CommandLineBuilder builder)
            where TCommand : ICommand, new()
            where TParameter : new()
        {
            return builder.Register(new CommandInvoker(new TCommand(), ImmutableNameSet.Empty, typeof(TParameter)));
        }

        [NotNull]
        public static CommandLineBuilder Register<TCommand>(this CommandLineBuilder builder)
            where TCommand : ICommand, new()
        {
            return builder.Register(new CommandInvoker(new TCommand(), ImmutableNameSet.Empty, null));
        }

        [NotNull]
        public static CommandLineBuilder Register<TParameter>(this CommandLineBuilder builder, ICommand command, params string[] names)
            where TParameter : new()
        {
            return builder.Register(new CommandInvoker(command, ImmutableNameSet.Create(names), typeof(TParameter)));
        }

        [NotNull]
        public static CommandLineBuilder Register(this CommandLineBuilder builder, ICommand command, params string[] names)
        {
            return builder.Register(new CommandInvoker(command, ImmutableNameSet.Create(names), null));
        }

        [NotNull]
        public static CommandLineBuilder Register(this CommandLineBuilder builder, [NotNull] Action<object> action, [NotNull] params string[] names)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (names == null) throw new ArgumentNullException(nameof(names));
            if (!names.Any()) throw new ArgumentException(paramName: nameof(names), message: "You need to specify at least one name.");

            return builder.Register(new CommandInvoker(new SimpleCommand(action), ImmutableNameSet.Create(names), null));
        }

        public static CommandLineBuilder RegisterHelpCommand(this CommandLineBuilder builder) => builder.Register<HelpCommand, HelpCommandParameter>();
    }
}