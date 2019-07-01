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
using Reusable.Commander.DependencyInjection;
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
            var counters = new ConcurrentDictionary<NameSet, int>();

            var commands =
                ImmutableList<CommandModule>
                    .Empty
                    .Add(new NameSet("a", "b"), ExecuteHelper.Count<TestCommandLine>(counters));

            using (var context = CreateContext(commands))
            {
                await context.Executor.ExecuteAsync<object>("a", default, context.CommandFactory);
                await context.Executor.ExecuteAsync<object>("b", default, context.CommandFactory);

                Assert.Equal(2, counters[NameSet.FromName("a")]);
            }
        }

        [Fact]
        public async Task CanExecuteMultipleCommands()
        {
            var counters = new ConcurrentDictionary<NameSet, int>();

            var commands =
                ImmutableList<CommandModule>
                    .Empty
                    .Add(new NameSet("a"), ExecuteHelper.Count<TestCommandLine>(counters))
                    .Add(new NameSet("b"), ExecuteHelper.Count<TestCommandLine>(counters));

            using (var context = CreateContext(commands))
            {
                await context.Executor.ExecuteAsync<object>("a|b", default, context.CommandFactory);

                Assert.Equal(1, counters[NameSet.FromName("a")]);
                Assert.Equal(1, counters[NameSet.FromName("b")]);
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
            var values = new Dictionary<NameSet, object>();
            var commands = ImmutableList<CommandModule>.Empty.Add(new NameSet("test"), new ExecuteCallback<TestCommandLine, object>((id, commandLine, context, token) =>
            {
                values[nameof(TestCommandLine.Bool)] = commandLine.Bool;
                values[nameof(TestCommandLine.BoolWithDefaultValue)] = commandLine.BoolWithDefaultValue;
                values[nameof(TestCommandLine.String)] = commandLine.String;
                values[nameof(TestCommandLine.StringWithDefaultValue)] = commandLine.StringWithDefaultValue;
                values[nameof(TestCommandLine.Int32)] = commandLine.Int32;
                values[nameof(TestCommandLine.Int32WithDefaultValue)] = commandLine.Int32WithDefaultValue;
                values[nameof(TestCommandLine.NullableInt32)] = commandLine.NullableInt32;
                return Task.CompletedTask;
            }));

            using (var context = CreateContext(commands))
            {
                await context.Executor.ExecuteAsync<object>("test", default, context.CommandFactory);

                Assert.Equal(false, values[nameof(TestCommandLine.Bool)]);
                Assert.Equal(true, values[nameof(TestCommandLine.BoolWithDefaultValue)]);
                Assert.Equal(null, values[nameof(TestCommandLine.String)]);
                Assert.Equal("foo", values[nameof(TestCommandLine.StringWithDefaultValue)]);
                Assert.Equal(0, values[nameof(TestCommandLine.Int32)]);
                Assert.Equal(3, values[nameof(TestCommandLine.Int32WithDefaultValue)]);
                Assert.Equal(null, values[nameof(TestCommandLine.NullableInt32)]);
            }
        }

        [Fact]
        public async Task Can_parse_supported_types()
        {
            var values = new Dictionary<NameSet, object>();
            var commands = ImmutableList<CommandModule>.Empty.Add(new NameSet("test"), new ExecuteCallback<TestCommandLine, object>((id, commandLine, context, token) =>
            {
                values[nameof(TestCommandLine.Bool)] = commandLine.Bool;
                values[nameof(TestCommandLine.String)] = commandLine.String;
                values[nameof(TestCommandLine.Int32)] = commandLine.Int32;
                values[nameof(TestCommandLine.DateTime)] = commandLine.DateTime;
                values[nameof(TestCommandLine.ListOfInt32)] = commandLine.ListOfInt32;
                return Task.CompletedTask;
            }));

            using (var context = CreateContext(commands))
            {
                await context.Executor.ExecuteAsync<object>("test -bool -string bar -int32 123 -datetime \"2019-07-01\" -listofint32 1 2 3", default, context.CommandFactory);

                Assert.Equal(true, values[nameof(TestCommandLine.Bool)]);
                Assert.Equal("bar", values[nameof(TestCommandLine.String)]);
                Assert.Equal(123, values[nameof(TestCommandLine.Int32)]);
                Assert.Equal(new DateTime(2019, 7, 1), values[nameof(TestCommandLine.DateTime)]);
                Assert.Equal(new[] { 1, 2, 3 }, values[nameof(TestCommandLine.ListOfInt32)]);
            }
        }

        [Fact]
        public async Task Throws_AggregateException_for_faulted_commands()
        {
            var values = new Dictionary<NameSet, object>();
            var commands = ImmutableList<CommandModule>.Empty.Add(new NameSet("test"), new ExecuteCallback<TestCommandLine, object>((id, commandLine, context, token) => { throw new Exception("Blub!"); }));

            using (var context = CreateContext(commands))
            {
                await Assert.ThrowsAsync<AggregateException>(async () => await context.Executor.ExecuteAsync<object>("test", default, context.CommandFactory));
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

        internal class TestCommandLine : CommandLine
        {
            public TestCommandLine(CommandLineDictionary arguments) : base(arguments) { }

            public bool Bool => GetArgument(() => Bool);

            [DefaultValue(true)]
            public bool BoolWithDefaultValue => GetArgument(() => BoolWithDefaultValue);

            public string String => GetArgument(() => String);

            [DefaultValue("foo")]
            public string StringWithDefaultValue => GetArgument(() => StringWithDefaultValue);

            public int Int32 => GetArgument(() => Int32);

            public int? NullableInt32 => GetArgument(() => NullableInt32);

            [DefaultValue(3)]
            public int Int32WithDefaultValue => GetArgument(() => Int32WithDefaultValue);

            public DateTime DateTime => GetArgument(() => DateTime);

            public DateTime? NullableDateTime => GetArgument(() => NullableDateTime);

            [DefaultValue("2018/01/01")]
            public DateTime DateTimeWithDefaultValue => GetArgument(() => DateTimeWithDefaultValue);

            public IList<int> ListOfInt32 => GetArgument(() => ListOfInt32);
        }
    }
}