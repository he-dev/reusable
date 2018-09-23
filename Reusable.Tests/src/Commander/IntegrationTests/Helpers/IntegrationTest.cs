using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;
using Reusable.OmniLog;
using IContainer = Autofac.IContainer;

namespace Reusable.Tests.Commander.IntegrationTests
{    
    public abstract class IntegrationTest
    {
        private IDictionary<Type, ICommandBag> _bags;
        private IDisposable _testCleanup;
        
        protected ICommandLineExecutor Executor { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            _bags = new Dictionary<Type, ICommandBag>();
            var container = InitializeContainer(_bags);
            var scope = container.BeginLifetimeScope();

            Executor = scope.Resolve<ICommandLineExecutor>();

            _testCleanup = Disposable.Create(() =>
            {
                scope.Dispose();
                container.Dispose();
            });
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _testCleanup.Dispose();
        }

        protected void ExecuteAssert<TBag>(Action<TBag> assert)
        {
            assert((TBag) _bags[typeof(TBag)]);
        }

        private static IContainer InitializeContainer(IDictionary<Type, ICommandBag> bags)
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(new LoggerFactory())
                .As<ILoggerFactory>();

            builder
                .RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>));

            builder
                .RegisterModule(new CommanderModule(new[]
                {
                    typeof(Command1),
                    typeof(Command2)
                }));

            builder
                .RegisterInstance(bags)
                .As<IDictionary<Type, ICommandBag>>();

            return builder.Build();
        }        
    }
}