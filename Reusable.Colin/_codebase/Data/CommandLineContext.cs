using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Shelly.Data
{
    public class CommandLineContext
    {
        public CommandLineContext(CommandLine commandLine, object parameters, Action<string> log)
        {
            CommandLine = commandLine;
            Parameter = parameters;
            Log = log;
        }

        public CommandLineContext(CommandLine commandLine, Action<string> log)
            : this(commandLine, null, log)
        {
        }

        public CommandLine CommandLine { get; set; }

        public object Parameter { get; set; }

        public Action<string> Log { get; set; }
    }
}
