using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Candle.Tests.Integration
{
    [TestClass]
    public class CommandLineTests
    {
        [TestMethod]
        public void Builder_Add_RequiresUniqueCommands()
        {
            new Action(() =>
            {
                CommandLine.Explicit
                    .Add<Cmd2Command>()
                    .Add<Cmd2Command>()
                    .Build();
            }).Verify().Throws<ArgumentException>();
        }

        [TestMethod]
        public void Builder_Add_RequiresUniqueCommandParameters()
        {
            new Action(() =>
            {
                CommandLine.Explicit
                    .Add<Cmd3Command>()
                    .Build();
            }).Verify().Throws<ArgumentException>();
        }

        [TestMethod]
        public void Execute_ImplicitCommand()
        {
            var exitCode = CommandLine.Implicit
                .Add<Cmd1Command>()
                .Build()
                .Execute(Enumerable.Empty<string>());
            exitCode.Verify().IsEqual(21);
        }

        [TestMethod]
        public void Execute_ExplicitCommand()
        {
            var exitCode = CommandLine.Explicit
                .Add<Cmd1Command>()
                .Add<Cmd2Command>()
                .Build()
                .Execute(new[] { "cmd2", "-foo=3", "-b=3,4,5" });
            exitCode.Verify().IsEqual(15);
        }

        [TestMethod]
        public void Execute_RequiresMandatoryParameters()
        {
            var cmdLine = CommandLine.Explicit
                .Add<Cmd1Command>()
                .Add<Cmd2Command>()
                .Build();

            cmdLine.Execute(new[] { "cmd2" }).Verify().IsEqual(ExitCode.ArgumentNotFound);
        }



        internal class Cmd1Command : Command
        {
            public override int Execute()
            {
                return 21;
            }
        }

        internal class Cmd2Command : Command
        {
            [Parameter(Mandatory = true)]
            public int Foo { get; set; }

            [Parameter(ValueSeparator = ',')]
            [Names("bar", "b")]
            public List<int> Bar { get; set; }

            [Parameter]
            public string Quux { get; set; }

            public override int Execute()
            {
                return Foo + Bar.Sum();
            }
        }

        internal class Cmd3Command : Command
        {
            [Parameter]
            [Names("foo")]
            public int Foo { get; set; }

            [Parameter]
            [Names("foo")]
            public List<int> Bar { get; set; }

            public override int Execute()
            {
                return 0;
            }
        }
    }
}
