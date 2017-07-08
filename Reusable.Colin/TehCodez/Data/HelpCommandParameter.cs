using System.ComponentModel;
using JetBrains.Annotations;
using Reusable.CommandLine.Annotations;
using Reusable.CommandLine.Logging;

namespace Reusable.CommandLine.Data
{
    [PublicAPI]
    public class HelpCommandParameter : ConsoleCommandParameter
    {
        [Parameter(Position = 1, Required = false)]
        [Description("Display command usage.")]
        public string Command { get; set; }
    }

    public class ConsoleCommandParameter
    {
        [NotNull]
        public CommandContainer Commands { get; set; }

        [NotNull]
        public ILogger Logger { get; set; }
    }
}