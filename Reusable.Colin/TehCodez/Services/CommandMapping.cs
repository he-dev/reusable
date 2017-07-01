using System;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.CommandLine.Collections;

namespace Reusable.CommandLine.Services
{
    public class CommandMapping
    {
        public CommandMapping([NotNull] ICommand command, [NotNull] IImmutableNameSet name, Type parameterType)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            Command = command ?? throw new ArgumentNullException(nameof(command));

            Name =
                name.Any()
                    ? ImmutableNameSet.Create(name)
                    : ImmutableNameSet.From(command);

            ParameterFactory = new CommandParameterFactory(parameterType);
        }

        [PublicAPI]
        [NotNull]
        public IImmutableNameSet Name { get; }

        [PublicAPI]
        [NotNull]
        public ICommand Command { get; }

        [PublicAPI]
        [NotNull]
        public CommandParameterFactory ParameterFactory { get; }
    }
}