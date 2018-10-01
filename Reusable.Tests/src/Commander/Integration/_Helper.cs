using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Reusable.Commander;
using Reusable.Commander.Commands;
using Reusable.OmniLog;
using IContainer = Autofac.IContainer;

namespace Reusable.Tests.Commander.Integration
{
    using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    internal static class Helper
    {
        public static TestContext CreateContext(Action<CommandRegistrationBuilder> commands)
        {
            var container = InitializeContainer(commands);
            var scope = container.BeginLifetimeScope();

            var executor = scope.Resolve<ICommandLineExecutor>();

            return new TestContext(Disposable.Create(() =>
                {
                    scope.Dispose();
                    container.Dispose();
                }),
                executor
            );
        }

        internal static ExecuteCallback<TBag> TrackBag<TBag>(BagTracker bags) where TBag : ICommandBag, new()
        {
            return (name, bag, cancellationToken) =>
            {
                bags.Add(name, bag);
                return Task.CompletedTask;
            };
        }

        internal static ExecuteCallback<TBag> Execute<TBag>(Action<Identifier, TBag, CancellationToken> execute) where TBag : ICommandBag, new()
        {
            return (name, bag, cancellationToken) =>
            {
                execute(name, bag, cancellationToken);
                return Task.CompletedTask;
            };
        }

        private static IContainer InitializeContainer(Action<CommandRegistrationBuilder> commands)
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(new LoggerFactory())
                .As<ILoggerFactory>();

            builder
                .RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>));

            builder
                .RegisterModule(new CommanderModule(commands));

            return builder.Build();
        }
    }

    internal class TestContext : IDisposable
    {
        private readonly IDisposable _disposer;

        public TestContext(IDisposable disposer, ICommandLineExecutor executor)
        {
            _disposer = disposer;
            Executor = executor;
        }

        public ICommandLineExecutor Executor { get; }

        public void Dispose() => _disposer.Dispose();
    }

    internal class BagTracker
    {
        private readonly IDictionary<Identifier, ICommandBag> _bags = new Dictionary<Identifier, ICommandBag>();

        public void Add(Identifier commandId, ICommandBag bag) => _bags.Add(commandId, bag);

        public void Assert<T>(Identifier commandId, Action<T> assert) where T : ICommandBag, new()
        {
            if (_bags.TryGetValue(commandId, out var bag))
            {
                assert((T) bag);
            }
            else
            {
                Fail($"There is no bag for '{commandId.Default.ToString()}'.");
            }
        }
    }
}