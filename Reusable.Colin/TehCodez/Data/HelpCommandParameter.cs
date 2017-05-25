using System.ComponentModel;
using JetBrains.Annotations;
using Reusable.Colin.Annotations;

namespace Reusable.Colin.Data
{
    [PublicAPI]
    public class HelpCommandParameter
    {
        [Parameter(Position = 1, Required = false)]
        [Description("Display command usage.")]
        public string Command { get; set; }
    }
}