using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;
using Reusable.OmniLog;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Reusable.Commander.Annotations;
using Reusable.IO;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;
using IContainer = Autofac.IContainer;

namespace Reusable.Tests.Commander
{
    public partial class IntegrationTest
    {
        private IDictionary<Type, ICommandBag> _bags;
        private IDisposable _testCleanup;
        
        private ICommandLineExecutor Executor { get; set; }

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

        private void ExecuteAssert<TBag>(Action<TBag> assert)
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