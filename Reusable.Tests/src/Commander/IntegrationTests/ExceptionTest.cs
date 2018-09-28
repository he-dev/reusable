using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;
using Reusable.Commander.Commands;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;
using static Reusable.Tests.Commander.Helper;

namespace Reusable.Tests.Commander.IntegrationTests
{
    [TestClass]
    public class ExceptionTest
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
        public void Add_DuplicateCommandNames_Throws()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", CreateExecuteCallback<SimpleBag>(bags))
                            .Add("c", CreateExecuteCallback<SimpleBag>(bags))
                    ))
                    {
                    }
                },
                filter => filter.When(name: "^RegisterCommand"),
                inner => inner.When(name: "^DuplicateCommandName--")
            );
        }

        [TestMethod]
        public void Add_DuplicateParameterNames_Throws()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", CreateExecuteCallback<BagWithDuplicateParameter>(bags))
                    ))
                    {
                    }
                },
                filter => filter.When(name: "^DuplicateParameterName")
            );
        }

        [TestMethod]
        public void Add_InvalidParameterPosition_Throws()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", CreateExecuteCallback<BagWithInvalidParameterPosition>(bags))
                    ))
                    {
                    }
                },
                filter => filter.When(name: "^ParameterPositionException")
            );
        }

        [TestMethod]
        public void Add_UnsupportedParameterType_Throws()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", CreateExecuteCallback<BagWithUnsupportedParameterType>(bags))
                    ))
                    {
                    }
                },
                filter => filter.When(name: "^UnsupportedParameterTypeException")
            );
        }

        #endregion

        #region Command-line validation

        [TestMethod]
        public void ExecuteAsync_NoCommandName_Throws()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (var context = CreateContext(
                        commands => commands
                            .Add("c", CreateExecuteCallback<SimpleBag>(bags))
                    ))
                    {
                        context.Executor.ExecuteAsync("-a").GetAwaiter().GetResult();
                    }
                },
                filter => filter.When(name: "^InvalidCommandLine")
            );
        }

        [TestMethod]
        public void ExecuteAsync_NonExistingCommandName_Throws()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (var context = CreateContext(
                        commands => commands
                            .Add("c", CreateExecuteCallback<SimpleBag>(bags))
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