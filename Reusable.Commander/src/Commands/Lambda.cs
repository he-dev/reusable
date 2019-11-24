using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Commander.Commands
{
    public delegate Task ExecuteDelegate<in TParameter>(MultiName name, TParameter? parameter, CancellationToken cancellationToken = default) where TParameter : class, new();

    public class Lambda<TParameter> : Command<TParameter> where TParameter : class, new()
    {
        private readonly ExecuteDelegate<TParameter> _execute;

        public Lambda(ILogger<Lambda<TParameter>> logger, MultiName name, ExecuteDelegate<TParameter> execute) : base(logger, name) => _execute = execute;

        protected override Task ExecuteAsync(TParameter? parameter, CancellationToken cancellationToken)
        {
            return _execute(Name, parameter, cancellationToken);
        }
    }
}