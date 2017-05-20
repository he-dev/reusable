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
        public CommandLineContext(CommandLine commandLine, ArgumentLookup arguments)
        {
            CommandLine = commandLine;
            Arguments = arguments;
        }        

        public CommandLine CommandLine { get; }

        public ArgumentLookup Arguments { get; }
    }
}
