using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Commander.Commands
{
    public delegate Task ExecuteCallback<in T>(Identifier commandId, T bag, CancellationToken cancellationToken = default) where T : ICommandParameter, new();

    public class Lambda<TParameter> : Command<TParameter, object> where TParameter : ICommandParameter, new()
    {
        private readonly ExecuteCallback<TParameter> _execute;

        public delegate Lambda<T> Factory<T>([NotNull] Identifier id, [NotNull] ExecuteCallback<TParameter> execute) where T : ICommandParameter, new();

        public Lambda(CommandServiceProvider<Lambda<TParameter>> serviceProvider, [NotNull] Identifier id, [NotNull] ExecuteCallback<TParameter> execute)
            : base(serviceProvider, id)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }
    }
}