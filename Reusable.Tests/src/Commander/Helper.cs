using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Reusable.Commander;
using Reusable.Commander.Commands;
using Reusable.OmniLog;
using IContainer = Autofac.IContainer;

namespace Reusable.Tests.Commander
{
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

        internal static ExecuteCallback<T> CreateExecuteCallback<T>(BagTracker bags) where T : ICommandBag, new()
        {
            return (name, bag, cancellationToken) =>
            {
                bags.Add(name, bag);
                return Task.CompletedTask;
            };
        }
        
        internal static ExecuteCallback<T> Execute<T>(Action<SoftKeySet, T, CancellationToken> execute) where T : ICommandBag, new()
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
        private readonly IDictionary<SoftKeySet, ICommandBag> _bags = new Dictionary<SoftKeySet, ICommandBag>();

        public void Add(SoftKeySet name, ICommandBag bag) => _bags.Add(name, bag);

        public void Assert<T>(SoftKeySet name, Action<T> assert) where T : ICommandBag, new() => assert((T) _bags[name]);
    }
}