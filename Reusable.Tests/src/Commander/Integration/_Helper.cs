using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Autofac;
using Reusable.Commander;
using Reusable.Commander.Commands;
using Reusable.Commander.Services;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using IContainer = Autofac.IContainer;

namespace Reusable.Tests.Commander.Integration
{
    using static Xunit.Assert;

    internal static class Helper
    {
        public static TestContext CreateContext(IImmutableList<CommandModule> commandRegistrations, ExecuteExceptionCallback executeExceptionCallback = null)
        {
            var container = InitializeContainer(commandRegistrations, executeExceptionCallback);
            var scope = container.BeginLifetimeScope();

            return new TestContext(scope.Resolve<ICommandExecutor>(), Disposable.Create(() =>
            {
                scope.Dispose();
                container.Dispose();
            }));
        }

        private static IContainer InitializeContainer(IImmutableList<CommandModule> commandRegistrations, ExecuteExceptionCallback executeExceptionCallback = null)
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(new LoggerFactory())
                .As<ILoggerFactory>();

            builder
                .RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>));

            builder
                .RegisterModule(new CommanderModule(commandRegistrations));

            if (!(executeExceptionCallback is null))
            {
                builder
                    .RegisterInstance(executeExceptionCallback);
            }

            //builder
            //    .RegisterInstance((ExecuteExceptionCallback)(ex =>
            //    {
            //        Fail(ex.Message);
            //    }));

            return builder.Build();
        }
    }

    internal static class ExecuteHelper
    {
        // internal static ExecuteCallback<TParameter> Track<TParameter>(CommandParameterTracker commandParameters) where TParameter : ICommandParameter
        // {
        //     return (name, bag, cancellationToken) =>
        //     {
        //         commandParameters.Add(name, bag);
        //         return Task.CompletedTask;
        //     };
        // }

        internal static ExecuteCallback<TParameter> Count<TParameter>(ConcurrentDictionary<Identifier, int> counters) where TParameter : ICommandParameter
        {
            return (id, commandLine, cancellationToken) =>
            {
                counters[id] = counters.AddOrUpdate(id, _ => 1, (_, count) => count + 1);
                return Task.CompletedTask;
            };
        }

        internal static ExecuteCallback<TBag> Noop<TBag>() where TBag : ICommandParameter, new()
        {
            return (name, bag, cancellationToken) => Task.CompletedTask;
        }
    }

    internal class TestContext : IDisposable
    {
        private readonly IDisposable _disposer;

        public TestContext(ICommandExecutor executor, IDisposable disposer)
        {
            _disposer = disposer;
            Executor = executor;
        }

        public ICommandExecutor Executor { get; }

        public void Dispose() => _disposer.Dispose();
    }

    internal class CommandParameterTracker
    {
        private readonly IDictionary<Identifier, ICommandParameter> _bags = new Dictionary<Identifier, ICommandParameter>();

        public void Add(Identifier commandId, ICommandParameter bag) => _bags.Add(commandId, bag);

        public void Assert<T>(Identifier commandId, Action<T> assert) where T : ICommandParameter, new()
        {
            if (_bags.TryGetValue(commandId, out var bag))
            {
                assert((T)bag);
            }
            else
            {
                False(true, $"There is no bag for '{commandId.Default.ToString()}'.");
            }
        }
    }
}