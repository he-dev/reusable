using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Reusable.OmniLog;
using Reusable.Tester;
using IContainer = Autofac.IContainer;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace Reusable.Commander.Tests
{
    [TestClass]
    public class CommandLineExecutorTest
    {
        private static TestLocal InitializeCommander(ICommandRegistrationContainer registrations, Action<IConsoleCommand> assert)
        {
            var memoryRx = MemoryRx.Create();
            var loggerFactory = new LoggerFactory(memoryRx);

            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(loggerFactory)
                .As<ILoggerFactory>();

            builder
                .RegisterInstance(assert);

            builder
                .RegisterModule(new CommanderModule(registrations));


            var container = builder.Build();
            var scope = container.BeginLifetimeScope();

            return new TestLocal(container, scope)
            {
                MemoryRx = memoryRx,
                Executor = scope.Resolve<ICommandLineExecutor>(),
                Assert = assert
            };
        }

        private class TestLocal : IDisposable
        {
            private readonly IContainer _container;
            private readonly ILifetimeScope _scope;

            public TestLocal(IContainer container, ILifetimeScope scope)
            {
                _container = container;
                _scope = scope;
            }

            public MemoryRx MemoryRx { get; set; }

            public ICommandLineExecutor Executor { get; set; }

            public Action<IConsoleCommand> Assert { get; set; }

            public void Dispose()
            {
                _container.Dispose();
                _scope.Dispose();
            }
        }

        [TestMethod]
        public void ExecuteAsync_VariousArguments_Mapped()
        {
            var assertExecuteMock = new Mock<Action<IConsoleCommand>>();
            assertExecuteMock.Setup(assert => assert(It.IsAny<TestCommandWithVariousTypes>())).Callback(new Action<IConsoleCommand>(cmd =>
            {
                var test = cmd as TestCommandWithVariousTypes;
                Assert.AreEqual("abc", test.StringProperty);
                Assert.AreEqual(123, test.Int32Property);
                Assert.AreEqual(true, test.BooleanPropertey);
                Assert.AreEqual(new DateTime(2017, 5, 1), test.DateTimeProperty);
                CollectionAssert.AreEqual(new[] { 4, 5, 6 }, test.Int32ArrayProperty);
            }));

            var commands = CommandRegistrationContainer.Empty.Register<TestCommandWithVariousTypes>();

            using (var local = InitializeCommander(commands, assertExecuteMock.Object))
            {
                var commandLineString = @"test -stringproperty abc -int32property 123 -datetimeproperty ""2017/5/1"" -booleanproperty false -int32arrayproperty 4 5 6 -justproperty noop";
                var executedCommandNames = local.Executor.ExecuteAsync(commandLineString, CancellationToken.None).GetAwaiter().GetResult();
                Assert.That.Collection().AreEqual(new[] { SoftKeySet.Create("test") }, executedCommandNames);
                assertExecuteMock.Verify(assert => assert(It.IsAny<IConsoleCommand>()), Times.Once);
            }
        }

        [TestMethod]
        public void ExecuteAsync_PositionalArgument_Mapped()
        {
            var assertExecuteMock = new Mock<Action<IConsoleCommand>>();
            assertExecuteMock.Setup(assert => assert(It.IsAny<TestCommandWithPosition>())).Callback(new Action<IConsoleCommand>(cmd =>
            {
                var test = cmd as TestCommandWithPosition;
                Assert.AreEqual("abc", test.PositionProperty);
            }));

            var commands = CommandRegistrationContainer.Empty.Register<TestCommandWithPosition>();

            using (var local = InitializeCommander(commands, assertExecuteMock.Object))
            {
                var commandLineString = @"test abc";
                var executedCommandNames = local.Executor.ExecuteAsync(commandLineString, CancellationToken.None).GetAwaiter().GetResult();
                Assert.That.Collection().AreEqual(new[] { SoftKeySet.Create("test") }, executedCommandNames);
                assertExecuteMock.Verify(assert => assert(It.IsAny<IConsoleCommand>()), Times.Once);
            }
        }

        [TestMethod]
        public void ExecuteAsync_WithFlag_Mapped()
        {
            var assertExecuteMock = new Mock<Action<IConsoleCommand>>();
            assertExecuteMock.Setup(assert => assert(It.IsAny<TestCommandWithFlag>())).Callback(new Action<IConsoleCommand>(cmd =>
            {
                var test = cmd as TestCommandWithFlag;
                Assert.IsTrue(test.Flag1);
                Assert.IsFalse(test.Flag2);
            }));

            var commands = CommandRegistrationContainer.Empty.Register<TestCommandWithFlag>();

            using (var local = InitializeCommander(commands, assertExecuteMock.Object))
            {
                var commandLineString = @"test -flag1 -flag2 false";
                var executedCommandNames = local.Executor.ExecuteAsync(commandLineString, CancellationToken.None).GetAwaiter().GetResult();
                Assert.That.Collection().AreEqual(new[] { SoftKeySet.Create("test") }, executedCommandNames);
                assertExecuteMock.Verify(assert => assert(It.IsAny<IConsoleCommand>()), Times.Once);
            }
        }

        [Alias("test")]
        private class TestCommandWithVariousTypes : TestCommand
        {
            public TestCommandWithVariousTypes(ILoggerFactory loggerFactory, Action<IConsoleCommand> assert) : base(loggerFactory, assert)
            {
            }

            [Parameter]
            public string StringProperty { get; set; }

            [Parameter]
            public int Int32Property { get; set; }

            [DefaultValue(true)]
            [Parameter]
            public bool BooleanPropertey { get; set; }

            [Parameter]
            public int[] Int32ArrayProperty { get; set; }

            [Parameter]
            public DateTime DateTimeProperty { get; set; }

            [Parameter, Alias("JustProperty")]
            public string AliasProperty { get; set; }           
        }

        [Alias("test")]
        private class TestCommandWithPosition : TestCommand
        {
            public TestCommandWithPosition(ILoggerFactory loggerFactory, Action<IConsoleCommand> assert) : base(loggerFactory, assert)
            {
            }

            [Parameter(Position = 1)]
            public string PositionProperty { get; set; }
        }

        private class TestCommandWithDuplicateNames : ConsoleCommand
        {
            public TestCommandWithDuplicateNames(ILoggerFactory loggerFactory) : base(loggerFactory)
            {
            }

            public string Foo { get; set; }

            [Alias("foo")]
            public string Bar { get; set; }

            public override Task ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        [Alias("test")]
        private class TestCommandWithFlag : TestCommand
        {
            private readonly Action<IConsoleCommand> _assert;

            public TestCommandWithFlag(ILoggerFactory loggerFactory, Action<IConsoleCommand> assert) : base(loggerFactory, assert)
            {
                _assert = assert;
            }

            [Parameter]
            public bool Flag1 { get; set; }

            [Parameter, DefaultValue(true)]
            public bool Flag2 { get; set; }
        }

        private class TestCommand : ConsoleCommand
        {
            private readonly Action<IConsoleCommand> _assert;

            protected TestCommand(ILoggerFactory loggerFactory, Action<IConsoleCommand> assert) : base(loggerFactory)
            {
                _assert = assert;
            }

            public override Task ExecuteAsync(CancellationToken cancellationToken)
            {
                _assert(this);
                return Task.CompletedTask;
            }
        }
    }
}