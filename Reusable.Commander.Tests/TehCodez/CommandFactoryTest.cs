using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.OmniLog;
using Reusable.Tester;

namespace Reusable.Commander.Tests
{
    [TestClass]
    public class CommandFactoryTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            
            var commands =
                CommandCollection.Empty
                    .Add<TestCommand>(/* no IEnumerable<int> numbers*/);

            var testLogger = TestLogger.Create();

            var commandFactory = new CommandFactory(testLogger, commands, new CommandParameterMapper(testLogger,));

            var command = commandFactory.CreateCommand("test", new CommandLine());
        }

        private class TestCommand : ConsoleCommand
        {
            private readonly ICommandFactory _commandFactory;

            public TestCommand(ILogger logger, ICommandFactory commandFactory, IEnumerable<int> numbers) : base(logger)
            {
                _commandFactory = commandFactory;
            }

            public override Task ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
