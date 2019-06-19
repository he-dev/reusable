using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.Commander.Commands;
using Reusable.Data.Annotations;
using Reusable.Tests.Commander.Integration;
using Xunit;

namespace Reusable.Tests.Commander
{
    using static Helper;

    public class FeatureTest
    {
        [Fact]
        public async Task CanExecuteCommandByAnyName()
        {
            var counters = new ConcurrentDictionary<Identifier, int>();

            var commands =
                ImmutableList<CommandModule>
                    .Empty
                    .Add(new Identifier("a", "b"), ExecuteHelper.Count<ICommandArgumentGroup>(counters));

            using (var context = CreateContext(commands))
            {
                await context.Executor.ExecuteAsync<object>("a", default);
                await context.Executor.ExecuteAsync<object>("b", default);

                Assert.Equal(2, counters[Identifier.FromName("a")]);
            }
        }

        [Fact]
        public async Task CanExecuteMultipleCommands()
        {
            var counters = new ConcurrentDictionary<Identifier, int>();

            var commands =
                ImmutableList<CommandModule>
                    .Empty
                    .Add(new Identifier("a"), ExecuteHelper.Count<ICommandArgumentGroup>(counters))
                    .Add(new Identifier("b"), ExecuteHelper.Count<ICommandArgumentGroup>(counters));

            using (var context = CreateContext(commands))
            {
                await context.Executor.ExecuteAsync<object>("a|b", default);

                Assert.Equal(1, counters[Identifier.FromName("a")]);
                Assert.Equal(1, counters[Identifier.FromName("b")]);
            }
        }

        // [Fact]
        // public async Task CanMapParameterByName()
        // {
        //     var tracker = new CommandParameterTracker();
        //     using (var context = CreateContext(
        //         commands => commands
        //             .Add("c", ExecuteHelper.Track<BagWithoutAliases>(tracker))
        //     ))
        //     {
        //         await context.Executor.ExecuteAsync<object>("c -StringWithoutAlias=abc", default);
        //         tracker.Assert<BagWithoutAliases>(
        //             "c",
        //             bag =>
        //             {
        //                 Assert.False(bag.Async);
        //                 //Assert.IsFalse(bag.CanThrow);
        //                 Assert.Equal("abc", bag.StringWithoutAlias);
        //             }
        //         );
        //     }
        // }

        // [Fact]
        // public async Task CanMapParameterByPosition()
        // {
        //     var tracker = new CommandParameterTracker();
        //     using (var context = CreateContext(
        //         commands => commands.Add("c", ExecuteHelper.Track<BagWithPositionalValues>(tracker)),
        //         (ExecuteExceptionCallback)(ex => throw ex)
        //     ))
        //     {
        //         await context.Executor.ExecuteAsync<object>("c 3 kmh -ismetric", default);
        //         tracker.Assert<BagWithPositionalValues>("c", bag =>
        //         {
        //             Assert.Equal(3, bag.Speed);
        //             Assert.Equal("kmh", bag.Unit);
        //             Assert.Equal(true, bag.IsMetric);
        //         });
        //     }
        // }

        // [Fact]
        // public async Task CanMapParameterByAlias()
        // {
        //     var tracker = new CommandParameterTracker();
        //     using (var context = CreateContext(
        //         commands => commands
        //             .Add("c", ExecuteHelper.Track<BagWithAliases>(tracker))
        //     ))
        //     {
        //         await context.Executor.ExecuteAsync<object>("c -swa=abc", default);
        //         tracker.Assert<BagWithAliases>(
        //             "c",
        //             bag =>
        //             {
        //                 Assert.False(bag.Async);
        //                 //Assert.False(bag.CanThrow);
        //                 Assert.Equal("abc", bag.StringWithAlias);
        //             }
        //         );
        //     }
        // }

        [Fact]
        public async Task Uses_default_values_when_specified()
        {
            var values = new Dictionary<Identifier, object>();
            var commands = ImmutableList<CommandModule>.Empty.Add(new Identifier("test"), new ExecuteCallback<ITestArgumentGroup, object>((id, reader, context, token) =>
            {
                values[nameof(ITestArgumentGroup.Bool)] = reader.GetItem(x => x.Bool);
                values[nameof(ITestArgumentGroup.BoolWithDefaultValue)] = reader.GetItem(x => x.BoolWithDefaultValue);
                values[nameof(ITestArgumentGroup.String)] = reader.GetItem(x => x.String);
                values[nameof(ITestArgumentGroup.StringWithDefaultValue)] = reader.GetItem(x => x.StringWithDefaultValue);
                values[nameof(ITestArgumentGroup.Int32)] = reader.GetItem(x => x.Int32);
                values[nameof(ITestArgumentGroup.Int32WithDefaultValue)] = reader.GetItem(x => x.Int32WithDefaultValue);
                values[nameof(ITestArgumentGroup.NullableInt32)] = reader.GetItem(x => x.NullableInt32);
                return Task.CompletedTask;
            }));

            using (var context = CreateContext(commands))
            {
                await context.Executor.ExecuteAsync<object>("test", default);

                Assert.Equal(false, values[nameof(ITestArgumentGroup.Bool)]);
                Assert.Equal(true, values[nameof(ITestArgumentGroup.BoolWithDefaultValue)]);
                Assert.Equal(null, values[nameof(ITestArgumentGroup.String)]);
                Assert.Equal("foo", values[nameof(ITestArgumentGroup.StringWithDefaultValue)]);
                Assert.Equal(0, values[nameof(ITestArgumentGroup.Int32)]);
                Assert.Equal(3, values[nameof(ITestArgumentGroup.Int32WithDefaultValue)]);
                Assert.Equal(null, values[nameof(ITestArgumentGroup.NullableInt32)]);
            }
        }

        [Fact]
        public async Task Can_parse_supported_types()
        {
            var values = new Dictionary<Identifier, object>();
            var commands = ImmutableList<CommandModule>.Empty.Add(new Identifier("test"), new ExecuteCallback<ITestArgumentGroup, object>((id, reader, context, token) =>
            {
                values[nameof(ITestArgumentGroup.Bool)] = reader.GetItem(x => x.Bool);
                values[nameof(ITestArgumentGroup.String)] = reader.GetItem(x => x.String);
                values[nameof(ITestArgumentGroup.Int32)] = reader.GetItem(x => x.Int32);
                values[nameof(ITestArgumentGroup.DateTime)] = reader.GetItem(x => x.DateTime);
                values[nameof(ITestArgumentGroup.ListOfInt32)] = reader.GetItem(x => x.ListOfInt32);
                return Task.CompletedTask;
            }));

            using (var context = CreateContext(commands))
            {
                await context.Executor.ExecuteAsync<object>("test -bool -string bar -int32 123 -datetime \"2019-07-01\" -listofint32 1 2 3", default);

                Assert.Equal(true, values[nameof(ITestArgumentGroup.Bool)]);
                Assert.Equal("bar", values[nameof(ITestArgumentGroup.String)]);
                Assert.Equal(123, values[nameof(ITestArgumentGroup.Int32)]);
                Assert.Equal(new DateTime(2019, 7, 1), values[nameof(ITestArgumentGroup.DateTime)]);
                Assert.Equal(new[] { 1, 2, 3 }, values[nameof(ITestArgumentGroup.ListOfInt32)]);
            }
        }

        // [Fact]
        // public async Task CanCreateBagWithFlagValues()
        // {
        //     var tracker = new CommandParameterTracker();
        //     using (var context = CreateContext(
        //         commands => commands
        //             .Add("c", ExecuteHelper.Track<SimpleBag>(tracker))
        //     ))
        //     {
        //         await context.Executor.ExecuteAsync<object>("c -async -canthrow=true", default);
        //         tracker.Assert<SimpleBag>(
        //             "c",
        //             bag =>
        //             {
        //                 Assert.True(bag.Async);
        //                 //Assert.True(bag.CanThrow);
        //             }
        //         );
        //     }
        // }

        // [Fact]
        // public async Task CanCreateBagWithCommandLineValues()
        // {
        //     var tracker = new CommandParameterTracker();
        //     using (var context = CreateContext(
        //         commands => commands
        //             .Add("c", ExecuteHelper.Track<BagWithMappedValues>(tracker))
        //     ))
        //     {
        //         await context.Executor.ExecuteAsync<object>("c", default);
        //         tracker.Assert<BagWithMappedValues>(
        //             "c",
        //             bag => { }
        //         );
        //     }
        // }

        // [Fact]
        // public async Task Does_not_execute_command_when_cannot_execute()
        // {
        //     using (var context = CreateContext(
        //         commands => commands
        //             .Add<DisabledCommand>()
        //     ))
        //     {
        //         await context.Executor.ExecuteAsync<object>("dc", default);
        //     }
        // }

        // [Tags("dc")]
        // private class DisabledCommand : Command<SimpleBag, NullContext>
        // {
        //     public DisabledCommand([NotNull] CommandServiceProvider<DisabledCommand> serviceProvider, [CanBeNull] Identifier id = default)
        //         : base(serviceProvider, id) { }
        //
        //     protected override Task ExecuteAsync(SimpleBag parameter, NullContext context, CancellationToken cancellationToken)
        //     {
        //         throw new InvalidOperationException("This command must not execute.");
        //     }
        //
        //     protected override Task<bool> CanExecuteAsync(SimpleBag parameter, NullContext context, CancellationToken cancellationToken = default)
        //     {
        //         return Task.FromResult(false);
        //     }
        // }

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

        internal interface ITestArgumentGroup : ICommandArgumentGroup
        {
            bool Bool { get; set; }

            [DefaultValue(true)]
            bool BoolWithDefaultValue { get; set; }

            string String { get; set; }

            [DefaultValue("foo")]
            string StringWithDefaultValue { get; set; }

            int Int32 { get; set; }

            int? NullableInt32 { get; set; }

            [DefaultValue(3)]
            int Int32WithDefaultValue { get; set; }

            DateTime DateTime { get; set; }

            DateTime? NullableDateTime { get; set; }

            [DefaultValue("2018/01/01")]
            DateTime DateTimeWithDefaultValue { get; set; }

            IList<int> ListOfInt32 { get; set; }
        }
    }
}