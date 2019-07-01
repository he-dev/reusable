using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Autofac;
using Reusable.Commander;
using Reusable.Commander.Commands;
using Reusable.Commander.DependencyInjection;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using IContainer = Autofac.IContainer;

namespace Reusable.Tests.Commander.Integration
{
    using static Xunit.Assert;

    internal static class Helper
    {
        public static TestContext CreateContext(IImmutableList<CommandModule> commandRegistrations)
        {
            var container = InitializeContainer(commandRegistrations);
            var scope = container.BeginLifetimeScope();

            return new TestContext(scope.Resolve<ICommandExecutor>(), scope.Resolve<ICommandFactory>(), Disposable.Create(() =>
            {
                scope.Dispose();
                container.Dispose();
            }));
        }

        private static IContainer InitializeContainer(IImmutableList<CommandModule> commandRegistrations)
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

        internal static ExecuteCallback<TCommandLine, object> Count<TCommandLine>(ConcurrentDictionary<NameSet, int> counters) where TCommandLine : ICommandLine
        {
            return (id, commandLine, context, cancellationToken) =>
            {
                counters[id] = counters.AddOrUpdate(id, _ => 1, (_, count) => count + 1);
                return Task.CompletedTask;
            };
        }

        internal static ExecuteCallback<TCommandLine, object> Noop<TCommandLine>() where TCommandLine : ICommandLine
        {
            return (name, commandLine, context, cancellationToken) => Task.CompletedTask;
        }
    }

    internal class TestContext : IDisposable
    {
        private readonly IDisposable _disposer;

        public TestContext(ICommandExecutor executor, ICommandFactory commandFactory, IDisposable disposer)
        {
            _disposer = disposer;
            Executor = executor;
            CommandFactory = commandFactory;
        }

        public ICommandExecutor Executor { get; }

        public ICommandFactory CommandFactory { get; }

        public void Dispose() => _disposer.Dispose();
    }

//    internal class CommandParameterTracker
//    {
//        private readonly IDictionary<Identifier, ICommandArgumentGroup> _bags = new Dictionary<Identifier, ICommandLine>();
//
//        public void Add(Identifier commandId, ICommandArgumentGroup bag) => _bags.Add(commandId, bag);
//
//        public void Assert<T>(Identifier commandId, Action<T> assert) where T : ICommandArgumentGroup, new()
//        {
//            if (_bags.TryGetValue(commandId, out var bag))
//            {
//                assert((T)bag);
//            }
//            else
//            {
//                False(true, $"There is no bag for '{commandId.Default.ToString()}'.");
//            }
//        }
//    }
}