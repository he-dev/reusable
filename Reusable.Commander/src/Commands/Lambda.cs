using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander.Services;

namespace Reusable.Commander.Commands
{
    public delegate Task ExecuteCallback<T>(Identifier commandId, ICommandLineReader<T> parameters, CancellationToken cancellationToken = default) where T : ICommandParameter;

    public class Lambda<TParameter> : Command<TParameter, object> where TParameter : ICommandParameter
    {
        private readonly ExecuteCallback<TParameter> _execute;

        public delegate Lambda<T> Factory<T>([NotNull] Identifier id, [NotNull] ExecuteCallback<TParameter> execute) where T : ICommandParameter, new();

        public Lambda
        (
            [NotNull] CommandServiceProvider<Lambda<TParameter>> serviceProvider,
            [NotNull] Identifier id,
            [NotNull] ExecuteCallback<TParameter> execute
        )
            : base(serviceProvider, id)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }
    }
}