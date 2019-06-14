using System;
using System.Threading.Tasks;
using Reusable.Commander;
using Reusable.Exceptionizer;
using Reusable.Reflection;
using Xunit;

namespace Reusable.Tests.Commander.Integration
{
    using static Helper;

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

        [Fact]
        public void DisallowDuplicateCommandNames()
        {
            var ex = Assert.ThrowsAny<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", Track<SimpleBag>(bags))
                            .Add("c", Track<SimpleBag>(bags))
                    )) { }
                }
//                ,
//                filter => filter.When(name: "^RegisterCommand"),
//                inner => inner.When(name: "^DuplicateCommandName")
            );
        }

        [Fact]
        public void DisallowDuplicateParameterNames()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", Track<BagWithDuplicateParameter>(bags))
                    )) { }
                },
                filter => filter.When(name: "^RegisterCommand"),
                inner => inner.When(name: "^DuplicateParameterName")
            );
        }

        [Fact]
        public void DisallowNonSequentialParameterPosition()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", Track<BagWithInvalidParameterPosition>(bags))
                    )) { }
                },
                filter => filter.When(name: "^RegisterCommand"),
                inner => inner.When(name: "^ParameterPositionException")
            );
        }

        [Fact]
        public void DisallowUnsupportedParameterType()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", Track<BagWithUnsupportedParameterType>(bags))
                    )) { }
                },
                filter => filter.When(name: "^RegisterCommand"),
                inner => inner.When(name: "^UnsupportedParameterTypeException")
            );
        }

        [Fact]
        public void DisallowCommandLineWithoutCommandName()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (var context = CreateContext(
                        commands => commands
                            .Add("c", Track<SimpleBag>(bags))
                    ))
                    {
                        context.Executor.ExecuteAsync<object>("-a", default).GetAwaiter().GetResult();
                    }
                },
                filter => filter.When(name: "^InvalidCommandLine")
            );
        }

        [Fact]
        public void DisallowNonExistingCommandName()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (var context = CreateContext(
                        commands => commands
                            .Add("c", Track<SimpleBag>(bags))
                    ))
                    {
                        context.Executor.ExecuteAsync<object>("b", default).GetAwaiter().GetResult();
                    }
                },
                filter => filter.When(name: "^InvalidCommandLine")
            );
        }

        [Fact]
        public void Throws_when_required_parameter_not_specified()
        {
            var exception = default(Exception);

            using (var context = CreateContext(
                commands => commands
                    .Add("c", ExecuteNoop<BagWithRequiredValue>()), inner => { exception = inner; }
            ))
            {
                Assert.ThrowsException<TaskCanceledException>(() => context.Executor.ExecuteAsync<object>("c", default).GetAwaiter().GetResult());
                Assert.IsNotNull(exception);
            }
        }

        [Fact]
        public void DisallowCommandLineWithMissingPositionalParameter()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    using (var context = CreateContext(
                        commands => commands.Add("c", ExecuteNoop<BagWithPositionalValues>()),
                        (ExecuteExceptionCallback)(ex => throw ex)
                    ))
                    {
                        context.Executor.ExecuteAsync<object>("c 3", default).GetAwaiter().GetResult();
                    }
                }
                //filter => filter.When(name: "^ParameterMapping")
            );
        }
    }
}