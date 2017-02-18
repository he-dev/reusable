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
            Parameters = parameters;
            Log = log;
        }

        public CommandLine CommandLine { get; set; }

        public object Parameters { get; set; }

        public Action<string> Log { get; set; }
    }
}
