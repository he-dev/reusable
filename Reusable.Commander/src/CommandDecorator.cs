using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Commander
{
    [UsedImplicitly]
    [PublicAPI]
    public abstract class CommandDecorator : ICommand
    {
        protected CommandDecorator(ICommand command) => Command = command;

        protected ICommand Command { get; }

        public virtual MultiName Name => Command.Name;

        public Type ParameterType => Command.ParameterType;

        public abstract Task ExecuteAsync(object? parameter, CancellationToken cancellationToken);
    }
}