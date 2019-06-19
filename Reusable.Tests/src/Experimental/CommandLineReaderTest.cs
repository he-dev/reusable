using System.Collections.Generic;
using System.ComponentModel;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Data.Annotations;
using Xunit;

namespace Reusable.Tests.Experimental
{
    public class CommandLineReaderTest
    {
        [Fact]
        public void Can_read_various_parameters()
        {
            var commandLine = new CommandLine
            {
                { "files", "first.txt" },
                { "files", "second.txt" },
                { "build", "debug" },
                { "canWrite" },
                { "canBuild" },
            };

            var cmdln = new CommandLineReader<ITestArgumentGroup>(commandLine);

            var actualFiles = cmdln.GetItem(x => x.Files);
            var actualBuild = cmdln.GetItem(x => x.Build);
            var canWrite = cmdln.GetItem(x => x.CanWrite);
            var canBuild = cmdln.GetItem(x => x.CanBuild);
            //var async = cmdln.GetItem(x => x.Async);

            Assert.Equal(new[] { "first.txt", "second.txt" }, actualFiles);
            Assert.Equal("debug", actualBuild);
            Assert.Equal(true, canWrite);
            Assert.Equal(true, canBuild);
            //Assert.Equal(true, async);
        }        
        
        internal interface ITestArgumentGroup : ICommandArgumentGroup
        {
            [Tags("f")]
            List<string> Files { get; }

            string Build { get; }

            bool CanWrite { get; }

            [DefaultValue(true)]
            bool CanBuild { get; }
        }
    }
}