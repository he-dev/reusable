using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Marbles;

namespace Reusable.DoubleDash;

[UsedImplicitly]
[PublicAPI]
public abstract class CommandDecorator : ICommand, IDecorator<ICommand>
{
    protected CommandDecorator(ICommand command) => Decoratee = command;

    public ICommand Decoratee { get; }

    public virtual NameCollection NameCollection => Decoratee.NameCollection;

    public Type ParameterType => Decoratee.ParameterType;

    public abstract Task ExecuteAsync(object? parameter, CancellationToken cancellationToken);
}