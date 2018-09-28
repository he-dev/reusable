using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;
using Reusable.Commander.Commands;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Commander.IntegrationTests
{
    [TestClass]
    public class ExceptionTest : IntegrationTest
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

        [TestMethod]
        public void ctor_CommanderModule_ctor_DuplicateCommands_Throws()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", CreateExecuteCallback<SimpleBag>(bags))
                            .Add("c", CreateExecuteCallback<SimpleBag>(bags)), out _
                    )) { }
                },
                filter => filter.WhenName("DuplicateCommandNameException")
            );
        }

        // [TestMethod]
        // public void ctor_CommanderModule_ctor_DuplicateParameters_Throws()
        // {
        //     Assert.That.Throws<DynamicException>(
        //         () => new CommanderModule(new[] { typeof(Lambda<InvalidBag1>), typeof(Lambda<InvalidBag1>) }),
        //         filter => filter.WhenName("DuplicateParameterNameException")
        //     );
        // }
        //
        // [TestMethod]
        // public void ctor_CommanderModule_ctor_InvalidParameterPositions_Throws()
        // {
        //     Assert.That.Throws<DynamicException>(
        //         () => new CommanderModule(new[] { typeof(Lambda<InvalidBag2>) }),
        //         filter => filter.WhenName("ParameterPositionException")
        //     );
        // }
        //
        // [TestMethod]
        // public void ctor_CommanderModule_ctor_InvalidParameterTypes_Throws()
        // {
        //     Assert.That.Throws<DynamicException>(
        //         () => new CommanderModule(new[] { typeof(Lambda<InvalidBag3>) }),
        //         filter => filter.WhenName("UnsupportedParameterTypeException")
        //     );
        // }
        //
        // [TestMethod]
        // public void ExecuteAsync_NoCommandName_Throws()
        // {
        //     Assert.That.Throws<DynamicException>(
        //         () => Executor.ExecuteAsync("-a").GetAwaiter().GetResult(),
        //         filter => filter.WhenName("^InvalidCommandLine")
        //     );
        // }
        //
        // [TestMethod]
        // public void ExecuteAsync_NonExistingCommandName_Throws()
        // {
        //     Assert.That.Throws<DynamicException>(
        //         () => Executor.ExecuteAsync("a").GetAwaiter().GetResult(),
        //         filter => filter.WhenName("^InvalidCommandLine")
        //     );
        // }
    }
}