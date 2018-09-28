using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;
using Reusable.Commander.Commands;
using Reusable.OmniLog;
using IContainer = Autofac.IContainer;

namespace Reusable.Tests.Commander.IntegrationTests
{
    public abstract class IntegrationTest
    {
        // private IDictionary<Type, ICommandBag> _bags;
        // private IDisposable _testCleanup;
        //
        // protected ICommandLineExecutor Executor { get; private set; }
        //
        // [TestInitialize]
        // public void TestInitialize()
        // {
        //     _bags = new Dictionary<Type, ICommandBag>();
        //     var container = InitializeContainer(_bags);
        //     var scope = container.BeginLifetimeScope();
        //
        //     Executor = scope.Resolve<ICommandLineExecutor>();
        //
        //     _testCleanup = Disposable.Create(
        //         () =>
        //         {
        //             scope.Dispose();
        //             container.Dispose();
        //         }
        //     );
        // }
        //
        // [TestCleanup]
        // public void TestCleanup()
        // {
        //     _testCleanup.Dispose();
        // }
        //
        // protected void ExecuteAssert<TBag>(Action<TBag> assert)
        // {
        //     assert((TBag)_bags[typeof(TBag)]);
        // }
        //
        // private static IContainer InitializeContainer(IDictionary<Type, ICommandBag> bags)
        // {
        //     var builder = new ContainerBuilder();
        //
        //     builder
        //         .RegisterInstance(new LoggerFactory())
        //         .As<ILoggerFactory>();
        //
        //     builder
        //         .RegisterGeneric(typeof(Logger<>))
        //         .As(typeof(ILogger<>));
        //
        //     builder
        //         .RegisterModule(new CommanderModule(commands => commands.Add<Command1>().Add<Command2>()));
        //
        //     builder
        //         .RegisterInstance(bags)
        //         .As<IDictionary<Type, ICommandBag>>();
        //
        //     return builder.Build();
        // }
        //
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

        protected static IDisposable CreateContext(Action<CommandRegistrationBuilder> commands, out ICommandLineExecutor executor)
        {
            var container = InitializeContainer(commands);
            var scope = container.BeginLifetimeScope();

            executor = scope.Resolve<ICommandLineExecutor>();

            return Disposable.Create(
                () =>
                {
                    scope.Dispose();
                    container.Dispose();
                }
            );
        }

        protected static LambdaExecuteCallback<T> CreateExecuteCallback<T>(BagTracker bags) where T : ICommandBag, new()
        {
            return (name, bag, cancellationToken) =>
            {
                bags.Add(name, bag);
                return Task.CompletedTask;
            };
        }

        protected class BagTracker
        {
            private readonly IDictionary<SoftKeySet, ICommandBag> _bags = new Dictionary<SoftKeySet, ICommandBag>();

            public void Add(SoftKeySet name, ICommandBag bag) => _bags.Add(name, bag);

            public void Assert<T>(SoftKeySet name, Action<T> assert) where T : ICommandBag, new() => assert((T)_bags[name]);
        }
    }
}