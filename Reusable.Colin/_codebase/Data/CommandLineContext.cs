using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Colin.Collections;

namespace Reusable.Colin.Data
{
    public class CommandLineContext
    {
        public CommandLineContext(CommandLine commandLine, ArgumentCollection argumentses, Action<string> log)
        {
            CommandLine = commandLine;
            Arguments = argumentses;
            Log = log;
        }

        public CommandLineContext(CommandLine commandLine, Action<string> log)
            : this(commandLine, null, log)
        {
        }

        public CommandLine CommandLine { get; }

        public ArgumentCollection Arguments { get; }

        public Action<string> Log { get; set; }
    }
}
