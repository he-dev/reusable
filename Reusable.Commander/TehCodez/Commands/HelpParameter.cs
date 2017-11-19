using System.ComponentModel;
using JetBrains.Annotations;
using Reusable.CommandLine;

namespace Reusable.Commander.Commands
{
    [PublicAPI]
    public class HelpParameter
    {
        public HelpParameter()
        {
            
        }
        //[CommandParameterProperty(Position = 1, Required = false)]
        [CanBeNull]
        [Position(1)]
        [DefaultValue(false)]
        [Description("Display command usage.")]
        public string Command { get; set; }
    }    
}