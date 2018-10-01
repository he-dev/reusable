using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Commander.Integration
{
    [TestClass]
    public class ExceptionIntegrationTest
    {
        // It's no longer possible to register any type so this test is irrelevant now.
        // [TestMethod]
        // public void ctor_CommanderModule_ctor_InvalidCommandType_Throws()
        // {
        //     Assert.That.Throws<DynamicException>(
        //         () => new CommanderModule(new[] {typeof(string)}),
        //         filter => filter.WhenName("CommandTypeException")
        //     );
        // }

    #region Command and parameter validation

        [TestMethod]
        public void DisallowDuplicateCommandNames()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (Helper.CreateContext(
                        commands => commands
                            .Add("c", Helper.TrackBag<SimpleBag>(bags))
                            .Add("c", Helper.TrackBag<SimpleBag>(bags))
                    )) { }
                },
                filter => filter.When(name: "^RegisterCommand"),
                inner => inner.When(name: "^DuplicateCommandName")
            );
        }

        [TestMethod]
        public void DisallowDuplicateParameterNames()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (Helper.CreateContext(
                        commands => commands
                            .Add("c", Helper.TrackBag<BagWithDuplicateParameter>(bags))
                    )) { }
                },
                filter => filter.When(name: "^RegisterCommand"),
                inner => inner.When(name: "^DuplicateParameterName")
            );
        }

        [TestMethod]
        public void DisallowNonSequentialParameterPosition()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (Helper.CreateContext(
                        commands => commands
                            .Add("c", Helper.TrackBag<BagWithInvalidParameterPosition>(bags))
                    )) { }
                },
                filter => filter.When(name: "^RegisterCommand"),
                inner => inner.When(name: "^ParameterPositionException")
            );
        }

        [TestMethod]
        public void DisallowUnsupportedParameterType()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (Helper.CreateContext(
                        commands => commands
                            .Add("c", Helper.TrackBag<BagWithUnsupportedParameterType>(bags))
                    )) { }
                },
                filter => filter.When(name: "^RegisterCommand"),
                inner => inner.When(name: "^UnsupportedParameterTypeException")
            );
        }

    #endregion

    #region Command-line validation

        [TestMethod]
        public void DisallowCommandLineWithoutCommandName()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (var context = Helper.CreateContext(
                        commands => commands
                            .Add("c", Helper.TrackBag<SimpleBag>(bags))
                    ))
                    {
                        context.Executor.ExecuteAsync("-a").GetAwaiter().GetResult();
                    }
                },
                filter => filter.When(name: "^InvalidCommandLine")
            );
        }

        [TestMethod]
        public void DisallowNonExistingCommandName()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (var context = Helper.CreateContext(
                        commands => commands
                            .Add("c", Helper.TrackBag<SimpleBag>(bags))
                    ))
                    {
                        context.Executor.ExecuteAsync("b").GetAwaiter().GetResult();
                    }
                },
                filter => filter.When(name: "^InvalidCommandLine")
            );
        }

    #endregion
    }
}