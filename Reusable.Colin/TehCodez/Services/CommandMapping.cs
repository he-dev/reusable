using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Data;
using Reusable.Colin.Logging;

namespace Reusable.Colin.Services
{
    public class CommandMapping
    {
        public CommandMapping([NotNull] ICommand command, [NotNull] ImmutableNameSet name, Type parameterType)
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
        public ImmutableNameSet Name { get; }

        [PublicAPI]
        [NotNull]
        public ICommand Command { get; }

        [PublicAPI]
        [NotNull]
        public CommandParameterFactory ParameterFactory { get; }
    }
}