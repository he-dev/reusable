using System.Collections.Generic;
using Candle;
using Candle.Writers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace RapidCommandLine.Tests.Commnads.HelpCommandTests
{
    [TestClass]
    public class ExecuteTests
    {
        [TestMethod]
        public void CanExecuteAsDefault()
        {
            var exitCode = CommandLine
                .Create()
                .Parse()
                .Register<HelpCommand>(cmd => cmd.IsDefault = true)
                .Execute();
            Assert.AreEqual(0, exitCode);
        }

        [TestMethod]
        public void CanDisplayCommandList()
        {
            var debugRenderer = new DebugWriter();
            var exitCode = CommandLine
                .Create(settings => settings.Renderers.Add(debugRenderer))
                .Parse("help", "-cmds")
                .Register<FooCommand>()
                //.Register<HelpCommand>(command => command. { cmdAfterMatch.CommandList.Is().Equal(true); }).AsDefault()
                .Execute();
            Assert.AreEqual(0, exitCode);

            debugRenderer.Messages.Count.Is().Equal(2);
            //output.Are().Equal
            //(
            //    "Test - Dummy test command.",
            //    "help|h|? - Display help."
            //);
        }
    }

    [Candle.Description("Dummy test command.")]
    internal class FooCommand : Command
    {
        [Parameter(IsRequired = true)]
        public int Foo { get; set; }

        [Parameter(IsRequired = false)]
        public string Bar { get; set; }

        [Parameter(ValueSeparator = ValueSeparator.Comma)]
        [Names("baz", "bz")]
        public List<int> Baz { get; set; }

        [Parameter]
        public string Quux { get; set; }

        public override int Execute()
        {
            return 0;
        }
    }
}
