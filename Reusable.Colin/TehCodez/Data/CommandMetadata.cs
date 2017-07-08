using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Services;

namespace Reusable.CommandLine.Data
{
    public class CommandMetadata
    {
        public CommandMetadata([NotNull] IImmutableNameSet name, [NotNull] ICommand command, [CanBeNull] Type parameterType)
        {
            CommandName = name ?? throw new ArgumentNullException(nameof(name));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            Parameter = CommandParameterFactory.CreateCommandParameterMetadata(parameterType);
        }

        [NotNull]
        public IImmutableNameSet CommandName { get; }

        [NotNull]
        public ICommand Command { get; }

        [NotNull]
        public ParameterMetadata Parameter { get; }
    }


}