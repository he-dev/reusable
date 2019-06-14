using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Commander.Services;
using Xunit;

namespace Reusable.Tests.Commander.Integration
{
    using static Helper;

    public class FeatureIntegrationTest
    {
        [Fact]
        public async Task CanExecuteCommandByAnyName()
        {
            var counters = new Dictionary<Identifier, int>();

            using (var context = CreateContext(
                commands => commands
                    .Add(Identifier.Create("a", "b"), ExecuteHelper.Count<SimpleBag>(counters))
            ))
            {
                await context.Executor.ExecuteAsync<object>("a", default);
                await context.Executor.ExecuteAsync<object>("b", default);

                Assert.Equal(2, counters["a"]);
            }
        }

        [Fact]
        public async Task CanExecuteMultipleCommands()
        {
            var counters = new Dictionary<Identifier, int>();

            using (var context = CreateContext(
                commands => commands
                    .Add(Identifier.Create("a"), ExecuteHelper.Count<SimpleBag>(counters))
                    .Add(Identifier.Create("b"), ExecuteHelper.Count<SimpleBag>(counters))
            ))
            {
                await context.Executor.ExecuteAsync<object>("a|b", default);

                Assert.Equal(1, counters["a"]);
                Assert.Equal(1, counters["b"]);
            }
        }

        [Fact]
        public async Task CanMapParameterByName()
        {
            var tracker = new BagTracker();
            using (var context = CreateContext(
                commands => commands
                    .Add("c", ExecuteHelper.Track<BagWithoutAliases>(tracker))
            ))
            {
                await context.Executor.ExecuteAsync<object>("c -StringWithoutAlias=abc", default);
                tracker.Assert<BagWithoutAliases>(
                    "c",
                    bag =>
                    {
                        Assert.False(bag.Async);
                        //Assert.IsFalse(bag.CanThrow);
                        Assert.Equal("abc", bag.StringWithoutAlias);
                    }
                );
            }
        }

        [Fact]
        public async Task CanMapParameterByPosition()
        {
            var tracker = new BagTracker();
            using (var context = CreateContext(
                commands => commands.Add("c", ExecuteHelper.Track<BagWithPositionalValues>(tracker)),
                (ExecuteExceptionCallback)(ex => throw ex)
            ))
            {
                await context.Executor.ExecuteAsync<object>("c 3 kmh -ismetric", default);
                tracker.Assert<BagWithPositionalValues>("c", bag =>
                {
                    Assert.Equal(3, bag.Speed);
                    Assert.Equal("kmh", bag.Unit);
                    Assert.Equal(true, bag.IsMetric);
                });
            }
        }

        [Fact]
        public async Task CanMapParameterByAlias()
        {
            var tracker = new BagTracker();
            using (var context = CreateContext(
                commands => commands
                    .Add("c", ExecuteHelper.Track<BagWithAliases>(tracker))
            ))
            {
                await context.Executor.ExecuteAsync<object>("c -swa=abc", default);
                tracker.Assert<BagWithAliases>(
                    "c",
                    bag =>
                    {
                        Assert.False(bag.Async);
                        //Assert.False(bag.CanThrow);
                        Assert.Equal("abc", bag.StringWithAlias);
                    }
                );
            }
        }

        [Fact]
        public async Task CanCreateBagWithDefaultValues()
        {
            var tracker = new BagTracker();
            using (var context = CreateContext(
                commands => commands
                    .Add("c", ExecuteHelper.Track<BagWithDefaultValues>(tracker))
            ))
            {
                await context.Executor.ExecuteAsync<object>("c", default);
                tracker.Assert<BagWithDefaultValues>(
                    "c",
                    bag =>
                    {
                        Assert.False(bag.Async);
                        //Assert.False(bag.CanThrow);
                        Assert.False(bag.BoolOnly);
                        Assert.False(bag.BoolWithDefaultValue1);
                        Assert.True(bag.BoolWithDefaultValue2);
                        Assert.Null(bag.StringOnly);
                        Assert.Equal("foo", bag.StringWithDefaultValue);
                        Assert.Equal(0, bag.Int32Only);
                        Assert.Null(bag.NullableInt32Only);
                        Assert.Equal(3, bag.Int32WithDefaultValue);
                        Assert.Equal(DateTime.MinValue, bag.DateTimeOnly);
                        Assert.Null(bag.NullableDateTime);
                        Assert.Equal(new DateTime(2018, 1, 1), bag.DateTimeWithDefaultValue);
                        Assert.Null(bag.ListOnly);
                    }
                );
            }
        }

        [Fact]
        public async Task CanCreateBagWithFlagValues()
        {
            var tracker = new BagTracker();
            using (var context = CreateContext(
                commands => commands
                    .Add("c", ExecuteHelper.Track<SimpleBag>(tracker))
            ))
            {
                await context.Executor.ExecuteAsync<object>("c -async -canthrow=true", default);
                tracker.Assert<SimpleBag>(
                    "c",
                    bag =>
                    {
                        Assert.True(bag.Async);
                        //Assert.True(bag.CanThrow);
                    }
                );
            }
        }

        [Fact]
        public async Task CanCreateBagWithCommandLineValues()
        {
            var tracker = new BagTracker();
            using (var context = CreateContext(
                commands => commands
                    .Add("c", ExecuteHelper.Track<BagWithMappedValues>(tracker))
            ))
            {
                await context.Executor.ExecuteAsync<object>("c", default);
                tracker.Assert<BagWithMappedValues>(
                    "c",
                    bag => { }
                );
            }
        }

        [Fact]
        public async Task Does_not_execute_command_when_cannot_execute()
        {
            using (var context = CreateContext(
                commands => commands
                    .Add<DisabledCommand>()
            ))
            {
                await context.Executor.ExecuteAsync<object>("dc", default);
            }
        }

        [Alias("dc")]
        private class DisabledCommand : ConsoleCommand<SimpleBag, NullContext>
        {
            public DisabledCommand([NotNull] CommandServiceProvider<DisabledCommand> serviceProvider, [CanBeNull] Identifier id = default)
                : base(serviceProvider, id) { }

            protected override Task ExecuteAsync(SimpleBag parameter, NullContext context, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("This command must not execute.");
            }

            protected override Task<bool> CanExecuteAsync(SimpleBag parameter, NullContext context, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(false);
            }
        }


        // [Fact]
        // public async Task ExecuteAsync_MultipleCommands_Executed()
        // {
        //     await Executor.ExecuteAsync("cmd1 -p03:true -p04:bar -p05 1 2 3 | cmd2 -p04:baz");
        //     
        //     ExecuteAssert<Bag1>(
        //         bag =>
        //         {
        //             Assert.False(bag.Bool1);
        //             Assert.True(bag.Bool2);
        //             Assert.True(bag.Bool3);
        //             Assert.Equal("bar", bag.String1);
        //             Assert.That.Collection().Equal(new[] { 1, 2, 3 }, bag.List1);
        //         }
        //     );
        //     
        //     ExecuteAssert<Bag2>(bag => { Assert.Equal("baz", bag.Property04); });
        // }
    }
}