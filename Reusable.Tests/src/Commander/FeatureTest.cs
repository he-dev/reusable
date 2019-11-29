using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Reusable.Commander.Annotations;
using Reusable.Commander.Helpers;
using Reusable.OmniLog.Abstractions;
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
                Command.Registration<CommandParameter>(MultiName.Create("a"), _ => executeCount++),
                Command.Registration<CommandParameter>(MultiName.Create("b"), _ => executeCount++)
            );

            await program.RunAsync("a|b");

            Assert.Equal(2, executeCount);
        }

        [Fact]
        public async Task Can_bind_args_by_name()
        {
            var param = default(SimpleParameter);

            using var program = TestProgram.Create
            (
                Command.Registration<SimpleParameter>(MultiName.Create("test"), p => param = p)
            );

            await program.RunAsync("test -bool -string bar -int32 123 -datetime \"2019-07-01\" -listofint32 1 2 3");

            Assert.NotNull(param);
            Assert.Equal(true, param.Bool);
            Assert.Equal("bar", param.String);
            Assert.Equal(123, param.Int32);
            Assert.Equal(new DateTime(2019, 7, 1), param.DateTime);
            Assert.Equal(new[] { 1, 2, 3 }, param.ListOfInt32);
        }


        [Fact]
        public async Task Can_bind_args_by_position()
        {
            var param = default(PositionParameter);

            using var program = TestProgram.Create
            (
                Command.Registration<PositionParameter>(MultiName.Create("test"), p => param = p)
            );

            await program.RunAsync("test bar foo");

            Assert.NotNull(param);
            Assert.Equal("bar", param.First);
            Assert.Equal("foo", param.Second);
        }

        [Fact]
        public async Task Can_bind_args_default_value()
        {
            var param = default(DefaultValueParameter);

            using var program = TestProgram.Create
            (
                Command.Registration<DefaultValueParameter>(MultiName.Create("test"), p => param = p)
            );

            await program.RunAsync("test");

            Assert.NotNull(param);
            Assert.Equal("foo", param.StringWithDefaultValue);
            Assert.Equal(3, param.Int32WithDefaultValue);
            Assert.Equal(new DateTime(2018, 2, 3), param.DateTimeWithDefaultValue);
        }

        [Fact]
        public async Task Can_bind_nullable()
        {
            var param = default(NullableValueParameter);

            using var program = TestProgram.Create
            (
                Command.Registration<NullableValueParameter>(MultiName.Create("test"), p => param = p)
            );

            await program.RunAsync("test -int32b 7");

            Assert.NotNull(param);
            Assert.False(param.Int32A.HasValue);
            Assert.True(param.Int32B.HasValue);
            Assert.Equal(7, param.Int32B.Value);
        }

        public async Task Can_bind_services()
        {
            var param = default(ServiceParameter);

            using var program = TestProgram.Create
            (
                Command.Registration<ServiceParameter>(MultiName.Create("test"), p => param = p)
            );

            await program.RunAsync("test");

            Assert.NotNull(param);
            Assert.NotNull(param.Logger);
            Assert.IsAssignableFrom<ILogger<ServiceParameter>>(param.Logger);
        }

        [Fact]
        public async Task Can_bind_context()
        {
            var param = default(ContextParameter);

            using var program = TestProgram.Create
            (
                Command.Registration<ContextParameter>(MultiName.Create("test"), p => param = p)
            );

            await program.RunAsync(new[] { "test" }, "foo");

            Assert.NotNull(param);
            Assert.Equal("foo", param.Blub);
        }

        private class SimpleParameter : CommandParameter
        {
            public bool Bool { get; set; }

            public string String { get; set; }

            public int Int32 { get; set; }

            public DateTime DateTime { get; set; }

            public IList<int> ListOfInt32 { get; set; }
        }

        private class PositionParameter : CommandParameter
        {
            [Position(1)]
            public string First { get; set; }

            [Position(2)]
            public string Second { get; set; }
        }

        private class DefaultValueParameter : CommandParameter
        {
            [DefaultValue("foo")]
            public string StringWithDefaultValue { get; set; }

            [DefaultValue(3)]
            public int Int32WithDefaultValue { get; set; }

            [DefaultValue("2018/02/03")]
            public DateTime DateTimeWithDefaultValue { get; set; }
        }

        private class ServiceParameter : CommandParameter
        {
            [Service]
            public ILogger<ServiceParameter> Logger { get; set; }
        }

        private class NullableValueParameter : CommandParameter
        {
            public int? Int32A { get; set; }

            public int? Int32B { get; set; }
        }

        private class ContextParameter : CommandParameter
        {
            [Context]
            public string Blub { get; set; }
        }
    }
}