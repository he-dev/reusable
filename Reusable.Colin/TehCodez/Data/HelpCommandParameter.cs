using System.ComponentModel;
using JetBrains.Annotations;
using Reusable.CommandLine.Annotations;
using Reusable.Loggex;

namespace Reusable.CommandLine.Data
{
    [PublicAPI]
    public class HelpCommandParameter : ConsoleCommandParameter
    {
        [Parameter(Position = 1, Required = false)]
        [Description("Display command usage.")]
        public string Command { get; set; }
    }

    public interface IConsoleCommandParameter
    {
        [NotNull]
        CommandContainer Commands { get; set; }

        [NotNull]
        ILogger Logger { get; set; }
    }

    public class ConsoleCommandParameter : IConsoleCommandParameter
    {
        public CommandContainer Commands { get; set; }

        public ILogger Logger { get; set; }
    }
}