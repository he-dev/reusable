using System.ComponentModel;
using JetBrains.Annotations;
using Reusable.Colin.Annotations;

namespace Reusable.Colin.Data
{
    [PublicAPI]
    public class HelpCommandParameter
    {
        [Parameter(Position = 1)]
        [CommandName("command-name", "cmd")]
        [Description("Display command usage.")]
        public string CommandName { get; set; }
    }
}