using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Threading.Tasks;
using Reusable.Commander.Commands;
using Reusable.Commander.DependencyInjection;
using Reusable.Commander.Helpers;
using Xunit;

namespace Reusable.Commander
{
    public class FeatureTest
    {
        [Fact]
        public async Task Can_execute_command_by_any_name()
        {
            var executeCount = 0;

            using var program = TestProgram.Create(Command.Registration<CommandParameter>(MultiName.Create("a", "b"), (name, param, token) =>
            {
                executeCount++;
                return Task.CompletedTask;
            }));

            await program.RunAsync("a");
            await program.RunAsync("b");

            Assert.Equal(2, executeCount);
        }

        [Fact]
        public async Task Can_execute_multiple_commands()
        {
            var executeCount = 0;

            using var program = TestProgram.Create
            (
                Command.Registration<CommandParameter>(MultiName.Create("a"), (name, param, token) =>
                {
                    executeCount++;
                    return Task.CompletedTask;
                }),
                Command.Registration<CommandParameter>(MultiName.Create("b"), (name, param, token) =>
                {
                    executeCount++;
                    return Task.CompletedTask;
                })
            );

            await program.RunAsync("a|b");

            Assert.Equal(2, executeCount);
        }

//        [Fact]
//        public async Task Can_bind_args_by_name()
//        {
//            var values = new Dictionary<NameCollection, object>();
//            var commands = ImmutableList<CommandModule>.Empty.Add(new NameCollection("test"), new ExecuteDelegate<TestCommandLine, object>((id, commandLine, context, token) =>
//            {
//                values[nameof(TestCommandLine.Bool)] = commandLine.Bool;
//                values[nameof(TestCommandLine.String)] = commandLine.String;
//                values[nameof(TestCommandLine.Int32)] = commandLine.Int32;
//                values[nameof(TestCommandLine.DateTime)] = commandLine.DateTime;
//                values[nameof(TestCommandLine.ListOfInt32)] = commandLine.ListOfInt32;
//                return Task.CompletedTask;
//            }));
//
//            using (var context = Helper.CreateContext(commands))
//            {
//                await context.Executor.ExecuteAsync<object>("test -bool -string bar -int32 123 -datetime \"2019-07-01\" -listofint32 1 2 3", default, context.CommandFactory);
//
//                Assert.Equal(true, values[nameof(TestCommandLine.Bool)]);
//                Assert.Equal("bar", values[nameof(TestCommandLine.String)]);
//                Assert.Equal(123, values[nameof(TestCommandLine.Int32)]);
//                Assert.Equal(new DateTime(2019, 7, 1), values[nameof(TestCommandLine.DateTime)]);
//                Assert.Equal(new[] { 1, 2, 3 }, values[nameof(TestCommandLine.ListOfInt32)]);
//            }
//        }

        [Fact]
        public async Task Can_bind_args_by_position() { }

        [Fact]
        public async Task Can_bind_args_default_value() { }

        public async Task Can_bind_services() { }

        public async Task Can_bind_context() { }

//        [Fact]
//        public async Task Throws_AggregateException_for_faulted_commands()
//        {
//            var values = new Dictionary<NameCollection, object>();
//            var commands = ImmutableList<CommandModule>.Empty.Add(new NameCollection("test"), new ExecuteDelegate<TestCommandLine, object>((id, commandLine, context, token) => { throw new Exception("Blub!"); }));
//
//            using (var context = Helper.CreateContext(commands))
//            {
//                await Assert.ThrowsAsync<AggregateException>(async () => await context.Executor.ExecuteAsync<object>("test", default, context.CommandFactory));
//            }
//        }


        internal class SimpleParameter : CommandParameter
        {
            public bool Bool { get; set; }

            public string String { get; set; }

            public int Int32 { get; set; }

            public DateTime DateTime { get; set; }

            public IList<int> ListOfInt32 { get; set; }
        }

        internal class DefaultValueParameter : CommandParameter
        {
            [DefaultValue("foo")]
            public string StringWithDefaultValue { get; set; }

            [DefaultValue(3)]
            public int Int32WithDefaultValue { get; set; }

            public DateTime DateTime { get; set; }

            [DefaultValue("2018/01/01")]
            public DateTime DateTimeWithDefaultValue { get; set; }
        }

        internal class NullableValueParameter : CommandParameter
        {
            public int? NullableInt32 { get; set; }

            public DateTime? NullableDateTime { get; set; }
        }
    }
}