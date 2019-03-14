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
    using static Xunit.Assert;

    internal static class Helper
    {
        public static TestContext CreateContext(Action<CommandRegistrationBuilder> commands, ExecuteExceptionCallback executeExceptionCallback = null)
        {
            var container = InitializeContainer(commands, executeExceptionCallback);
            var scope = container.BeginLifetimeScope();

            return new TestContext(scope.Resolve<ICommandLineExecutor>(), Disposable.Create(() =>
            {
                scope.Dispose();
                container.Dispose();
            }));
        }

        private static IContainer InitializeContainer(Action<CommandRegistrationBuilder> commands, ExecuteExceptionCallback executeExceptionCallback = null)
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
        internal static ExecuteCallback<TBag> Track<TBag>(BagTracker bags) where TBag : ICommandBag, new()
        {
            return (name, bag, cancellationToken) =>
            {
                bags.Add(name, bag);
                return Task.CompletedTask;
            };
        }

        internal static ExecuteCallback<TBag> Count<TBag>(IDictionary<Identifier, int> counters) where TBag : ICommandBag, new()
        {
            return (name, bag, cancellationToken) =>
            {
                counters[name] = counters.TryGetValue(name, out var count) ? count + 1 : 1;
                return Task.CompletedTask;
            };
        }
        
        internal static ExecuteCallback<TBag> Noop<TBag>() where TBag : ICommandBag, new()
        {
            return (name, bag, cancellationToken) => Task.CompletedTask;
        }

    }

    internal class TestContext : IDisposable
    {
        private readonly IDisposable _disposer;

        public TestContext(ICommandLineExecutor executor, IDisposable disposer)
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
                assert((T)bag);
            }
            else
            {
                False(true, $"There is no bag for '{commandId.Default.ToString()}'.");
            }
        }
    }
}